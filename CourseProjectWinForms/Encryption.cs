using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace CourseProjectWinForms
{
    public class SerpentCipher
    {
        public int AlphabetLength { get; set; }
        static readonly int BlockSize = 16; // bytes in a data-block
        static readonly int DefaultKeySize = 32;
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public SerpentCipher()
        {
            this.AlphabetLength = 256;
        }

        public bool Encrypt(string FilePath, byte[] Key, int Rounds, Mode Mode, EncryptionMode EncrMode)
        {
            FileManagement fm = new FileManagement();
            SerpentAlgorithm sa;
            byte[] saltBytes = new byte[BlockSize];
            rng.GetNonZeroBytes(saltBytes);
            byte[] iv = new byte[BlockSize];
            rng.GetBytes(iv);

            if (Mode == Mode.Standard)
                sa = new SerpentStandardMode();
            //else if (Mode == Mode.BitSlice)
            //    //sa = new SerpentBitSliceMode();
            //    MessageBox.Show("BitSliceMod is not implemented");
            else
            {
                MessageBox.Show("Выбранный режим алгоритма не реализован ");
                return false;
            }

            ErrorCode errorCode;
            int m = AlphabetLength;
            int position = 0;
            List<byte> InputFileFragment;
            string destFilePath = Path.ChangeExtension(FilePath, ".serpent");
            FileInfo fi = new FileInfo(FilePath);
            int fragmentSize = 1024 * 1024; // 1 MB нужно уметь извлекат ькорень 4 степени
            byte[] leadingBytes;
            string tempFilePath = FilePath + ".temp";
            string tempFilePath2 = FilePath + ".temp2";
            Encoding enc = new UTF8Encoding();
            sa.Rounds = Rounds;
            sa.BlockSize = BlockSize;
            byte[] previousBlock = new byte[0];

            byte[] roundsBytes = enc.GetBytes(Rounds.ToString());

            for (int i = 0; i < roundsBytes.Length; i++)
            {
                saltBytes[i] = roundsBytes[i];

                if (i >= roundsBytes.Length - 1)
                    saltBytes[i + 1] = 3;
            }

            byte[] ptFragment = new byte[BlockSize];
            object expandedKey = sa.MakeKey(Key);

            leadingBytes = new byte[BlockSize - (fi.Length % BlockSize)];

            for (int i = 0; i < leadingBytes.Length; i++)
                if (i != 0)
                    leadingBytes[i] = (byte)rnd.Next(0, AlphabetLength - 1);
                else
                    leadingBytes[i] = (byte)leadingBytes.Length;

            fm.UnshiftBytesToFile(FilePath, tempFilePath, leadingBytes, out errorCode); // добавляем во временный файл

            if (errorCode == ErrorCode.ExpandFileFailed)
            {
                MessageBox.Show("Файл не может быть изменён", "Error");
                return false;
            }

            do
            {
                if (position == 0) // добавить в файл информацию о расширении, кол-ве раундов и контрольной сумме
                {
                    List<byte> infoBytes = new List<byte>();

                    infoBytes.AddRange(saltBytes);

                    string extension = Path.GetExtension(FilePath);
                    extension = extension.Replace(".", "");
                    infoBytes.AddRange(enc.GetBytes(extension));
                    infoBytes.Add(3); // EOT byte
                    infoBytes.AddRange(enc.GetBytes(Rounds.ToString()));
                    infoBytes.Add(3);

                    if (BlockSize < infoBytes.Count - saltBytes.Length)
                    {
                        MessageBox.Show("Расширение файла слишком длинное");
                        return false;
                    }

                    byte[] rndBytes = new byte[BlockSize - (infoBytes.Count - saltBytes.Length)];
                    rng.GetNonZeroBytes(rndBytes);
                    infoBytes.AddRange(rndBytes);

                    fm.UnshiftBytesToFile(tempFilePath, tempFilePath2, infoBytes.ToArray(), out errorCode);

                    if (errorCode == ErrorCode.ExpandFileFailed)
                    {
                        MessageBox.Show("Файл не может быть изменён");
                        return false;
                    }

                    fm.DeleteTempFile(tempFilePath, out errorCode);

                    if (errorCode == ErrorCode.DeletingTempFileFailed) // в случае ошибки шифрование не удалось
                    {
                        MessageBox.Show("Временный файл не может быть удалён");
                        return false;
                    }

                    File.Move(tempFilePath2, tempFilePath);
                }

                InputFileFragment = fm.GetFileFragment(tempFilePath, position, fragmentSize, out errorCode).ToList();

                if (errorCode == ErrorCode.GetFileFailed)
                {
                    MessageBox.Show("Не удалось загрузить файл");
                    return false;
                }

                List<byte> OutputFileFragment = new List<byte>();

                // шифрование блока

                for (int i = 0; i < InputFileFragment.Count; i += BlockSize)
                {
                    if (EncrMode == EncryptionMode.ECB)
                    {
                        OutputFileFragment.AddRange(sa.BlockEncrypt(InputFileFragment.GetRange(i, BlockSize).ToArray(), 0, expandedKey));
                    }
                    else if (EncrMode == EncryptionMode.CBC)
                    {
                        if (position == 0 && i == 0) // инициализируем iv только 1 раз
                            previousBlock = iv;

                        byte[] plainText = InputFileFragment.GetRange(i, BlockSize).ToArray();
                        byte[] currBlock = plainText.XOR(previousBlock); // в открытый текст исключаем предыдущий зашифрованный блок
                        byte[] cipherText = sa.BlockEncrypt(currBlock, 0, expandedKey);
                        OutputFileFragment.AddRange(cipherText);
                        previousBlock = cipherText;
                    }
                    else
                    {
                        MessageBox.Show("Выбранный режим шифрования не реализован ", "Error");
                        return false;
                    }
                }

                // ==========================================

                fm.SaveFileFragment(destFilePath, position, OutputFileFragment.ToArray(), out errorCode); // сохранение другой части файла в целевой файл

                if (errorCode == ErrorCode.SaveFileFailed) // в случае ошибки шифрование не удалось
                {
                    MessageBox.Show("Файл не может быть сохранён ");
                    return false;
                }

                position += fragmentSize; // установить позицию следующего фрагмента

             //   EncryptionProgressChangedEventArgs encryptionProgressChangedEventData = new EncryptionProgressChangedEventArgs(((int)((double)position / (double)fi.Length * 100.0)), ActionType.Encryption); // inicjalizuję dane dla event handlera (obliczony postęp, typ - szyfrowanie czy deszyfrowanie)
               // OnEncryptionProgressChanging(encryptionProgressChangedEventData); // wywołuję zdarzenie z utworzonymi wcześniej parametrami

            }
            while (position <= fi.Length); 

            fm.DeleteTempFile(tempFilePath, out errorCode); // удалить временный файл

            if (errorCode == ErrorCode.DeletingTempFileFailed) // гшибка - не зашифровали
            {
                MessageBox.Show("Временный файл не может быть удалён");
                return false;
            }

            byte[] ivAndSalt = new byte[iv.Length + saltBytes.Length];
            iv.CopyTo(ivAndSalt, 0);
            saltBytes.CopyTo(ivAndSalt, iv.Length);

            fm.UnshiftBytesToFile(destFilePath, tempFilePath, ivAndSalt, out errorCode); // контрольная сумма

            if (errorCode == ErrorCode.ExpandFileFailed)
            {
                MessageBox.Show("Файл не может быть изменен");
                return false;
            }

            fm.DeleteTempFile(destFilePath, out errorCode); // удаляем временный файл

            if (errorCode == ErrorCode.DeletingTempFileFailed) // ошибка - не зашифровали
                MessageBox.Show("Временный файл не может быть удалён");

            File.Move(tempFilePath, destFilePath);

            return true;
        }


        //-----------------------------------------------------------

        public bool Decrypt(string FilePath, byte[] Key, int Rounds, Mode Mode, EncryptionMode EncrMode)
        {
            FileManagement fm = new FileManagement();
            SerpentAlgorithm sa;

            if (Mode == Mode.Standard)
                sa = new SerpentStandardMode();
            //else if (Mode == Mode.BitSlice)
            //    sa = new SerpentBitSliceMode();
            else
            {
                MessageBox.Show("Выбранный режим шифрования не реализован");
                return false;
            }

            ErrorCode errorCode;
            int m = AlphabetLength;
            int position = 0;
            string outputExtension;
            List<byte> InputFileFragment;
            string destFilePath = string.Empty;
            FileInfo fi = new FileInfo(FilePath);
            int fragmentSize = 1024 * 1024; // 1 MB
            Encoding enc = new UTF8Encoding();
            sa.Rounds = Rounds;
            sa.BlockSize = BlockSize;
            string tempFilePath = FilePath + ".temp";

            byte[] iv = fm.GetFileFragment(FilePath, 0, BlockSize, out errorCode);
            byte[] plainControlSum = fm.GetFileFragment(FilePath, BlockSize, BlockSize, out errorCode);
            byte[] roundBytesFromPtControlSum = new byte[0];
            byte[] previousBlock = new byte[0];

            if (errorCode == ErrorCode.GetFileFailed)
            {
                MessageBox.Show("Не удалось загрузить файл");
                return false;
            }

            List<byte> listRoundsBytesFromPtControlSum = new List<byte>();

            for (int i = 0; i < BlockSize; i++)
            {
                if (plainControlSum[i] != 3)
                    listRoundsBytesFromPtControlSum.Add(plainControlSum[i]);
                else
                {
                    roundBytesFromPtControlSum = listRoundsBytesFromPtControlSum.ToArray();
                    break;
                }
            }

            int readRoundsFromPtControlSum = 0;
            string strRoundsFromPtControlSum = enc.GetString(roundBytesFromPtControlSum);
            bool areRoundsFromPtControlSumParsable = int.TryParse(enc.GetString(roundBytesFromPtControlSum), out readRoundsFromPtControlSum);

            if (!areRoundsFromPtControlSumParsable || readRoundsFromPtControlSum > 64)
            {
                MessageBox.Show("Файл не зашифрован или зашифрован другим алгоритмом");
                return false;
            }
            else if (readRoundsFromPtControlSum != Rounds)
            {
                if (Mode != Mode.Standard)
                {
                   // MessageBoxResult result = MessageBox.Show(string.Format("Plik został zaszyfrowany inną liczbą rund ({0}) algorytmu Serpent niż podana ({1}), ale w trybie innym niż Standardowy nie można odszyfrować przy użyciu takiej liczby rund. Czy chcesz zmienić tryb na Standardowy i liczbę rund na {0}? (UWAGA: Operacja deszyfrowania może trwać bardzo długo dla dużych plików). ", readRoundsFromPtControlSum, Rounds), "Ostrzeżenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                   // if (result == MessageBoxResult.Yes)
                        return Decrypt(FilePath, Key, readRoundsFromPtControlSum, Mode.Standard, EncrMode);
                    //else
                    //    return false;
                }
                else
                {
                //    MessageBoxResult result = MessageBox.Show(string.Format("Wygląda na to, że plik został zaszyfrowany inną liczbą rund ({0}) algorytmu Serpent niż podana ({1}) lub plik w ogóle nie jest zaszyfrowany. Czy chcesz zmienić liczbę rund na {0}?", readRoundsFromPtControlSum, Rounds), "Ostrzeżenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                   // if (result == MessageBoxResult.Yes)
                    {
                        Rounds = readRoundsFromPtControlSum;
                        sa.Rounds = readRoundsFromPtControlSum;
                    }
                }
            }

            byte[] ptFragment = new byte[BlockSize];
            object expandedKey = sa.MakeKey(Key);

            fm.ShiftBytesFromFile(FilePath, tempFilePath, BlockSize * 2, out errorCode); // удалить из файла 2 блока - контрольную сумму и вектор инициализации

            if (errorCode == ErrorCode.ShiftFileFailed)
            {
                MessageBox.Show("Файл не может быть изменен");
                return false;
            }

            do
            {
                InputFileFragment = fm.GetFileFragment(tempFilePath, position, fragmentSize, out errorCode).ToList();

                if (errorCode == ErrorCode.GetFileFailed)
                {
                    MessageBox.Show("Не удалось загрузить файл");
                    return false;
                }

                List<byte> OutputFileFragment = new List<byte>();

                // дешифрование блока

                for (int i = 0; i < InputFileFragment.Count; i += BlockSize)
                {
                    if (EncrMode == EncryptionMode.ECB)
                    {
                        OutputFileFragment.AddRange(sa.BlockDecrypt(InputFileFragment.GetRange(i, BlockSize).ToArray(), 0, expandedKey));
                    }
                    else if (EncrMode == EncryptionMode.CBC)
                    {
                        if (position == 0 && i == 0)
                            previousBlock = iv;
                        byte[] cipherText = InputFileFragment.GetRange(i, BlockSize).ToArray();
                        byte[] currBlock = sa.BlockDecrypt(cipherText, 0, expandedKey);
                        byte[] plainText = currBlock.XOR(previousBlock);
                        OutputFileFragment.AddRange(plainText);
                        previousBlock = cipherText;
                    }
                    else
                    {
                        MessageBox.Show("Выбранный режи шифрования не реализован ");
                        return false;
                    }
                }

                // ===================

                if (position == 0) // если расшифрован первый фрагмент файла, то удаляем байты, добавленные во времяя шифрования из таблицы перед сохранением
                {
                    int shiftedbytes = OutputFileFragment[BlockSize * 2]; // 32 байта содержимого, расшифрованная контрольная сумма 0-15, расширение 16-31 и количество раундов
                    byte[] decryptedControlSum = new byte[BlockSize];
                    byte[] extBytes = new byte[1];
                    byte[] roundBytes = new byte[1];
                    int i = 0;

                    for (; i < BlockSize; i++) // прочитать контрольную сумму
                        decryptedControlSum[i] = OutputFileFragment[i];

                    List<byte> listExtbytes = new List<byte>();

                    for (; i < BlockSize * 2; i++) // читать расширение
                    {
                        if (OutputFileFragment[i] != 3)
                            listExtbytes.Add(OutputFileFragment[i]);
                        else
                        {
                            extBytes = listExtbytes.ToArray();
                            break;
                        }
                    }

                    i++;
                    List<byte> listRoundsBytes = new List<byte>();

                    for (; i < BlockSize * 2; i++)  // прочитать зашифрованное кол-во раундов
                    {
                        if (OutputFileFragment[i] != 3)
                            listRoundsBytes.Add(OutputFileFragment[i]);
                        else
                        {
                            roundBytes = listRoundsBytes.ToArray();
                            break;
                        }
                    }

                    outputExtension = enc.GetString(extBytes);
                    destFilePath = Path.ChangeExtension(FilePath, outputExtension);
                    int readRounds;
                    bool areRoundsParsable = int.TryParse(enc.GetString(roundBytes), out readRounds);

                    if (!plainControlSum.SequenceEqual(decryptedControlSum) || !areRoundsParsable || shiftedbytes > 16 || readRounds != Rounds)
                    {
                        fm.DeleteTempFile(tempFilePath, out errorCode);
                        if (errorCode == ErrorCode.DeletingTempFileFailed)
                            MessageBox.Show("Вреенный файл не может быть удален");

                        MessageBox.Show("Ключ недействителен ");

                        return false;
                    }

                    int shiftedBytesAndInfoBytes = shiftedbytes + BlockSize * 2;

                    for (; shiftedBytesAndInfoBytes > 0; shiftedBytesAndInfoBytes--)
                        OutputFileFragment.RemoveAt(0);
                }

                fm.SaveFileFragment(destFilePath, position, OutputFileFragment.ToArray(), out errorCode); // сохранение другой части файла в целевой файл

                if (errorCode == ErrorCode.SaveFileFailed) 
                {
                    MessageBox.Show("Файл не может быть сохранен");
                    return false;
                }

                position += fragmentSize; // ustaw pozycję kolejnego fragmentu
               // EncryptionProgressChangedEventArgs encryptionProgressChangedEventData = new EncryptionProgressChangedEventArgs(((int)((double)position / (double)fi.Length * 100.0)), ActionType.Decryption); // inicjalizuję dane dla event handlera (obliczony postęp, typ - szyfrowanie czy deszyfrowanie)
               // OnEncryptionProgressChanging(encryptionProgressChangedEventData); // wywołuję zdarzenie z utworzonymi wcześniej parametrami

            }
            while (position <= fi.Length); 

            fm.DeleteTempFile(tempFilePath, out errorCode); // удалить временный файл

            if (errorCode == ErrorCode.DeletingTempFileFailed)
                MessageBox.Show("Временный файл не может быть удален");

            return true;
        }

        public Random rnd = new Random();

        public byte[] RandomizeKey() // Генерация ключа
        {
            byte[] key = new byte[DefaultKeySize];
            rng.GetNonZeroBytes(key);

            return key;
        }

        //public event EncryptionProgressChangedEventHandler EncryptionProgressChanged; // tworzę zdarzenie wywoływane przy zmianie stanu operacji szyfrowania

        //protected virtual void OnEncryptionProgressChanging(EncryptionProgressChangedEventArgs e) // metoda upewniająca się, że parametry przyjmowane przez zdarzenie nie są puste
        //{
        //    if (EncryptionProgressChanged != null)
        //    {
        //        EncryptionProgressChanged(this, e);
        //    }
        //}
    }

    public enum ActionType
    {
        Encryption,
        Decryption,
        ChangingText,
    }

    public enum Mode
    {
        Standard,
        BitSlice
    }

    public enum EncryptionMode
    {
        ECB,
        CBC
    }

    public enum KeyMode
    {
        Chars,
        Bytes
    }

    public static partial class ExtensionMethods
    {
        public static byte[] XOR(this byte[] buffer1, byte[] buffer2)
        {
            for (int i = 0; i < buffer1.Length; i++)
                buffer1[i] ^= buffer2[i];

            return buffer1;
        }
    }

}

