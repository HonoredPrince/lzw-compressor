using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace SourceCode
{
    public class LZWClassificatorOld
    {
        private const int MAX_BITS = 16; //Tamanho máximo de bits de leitura
        private const int HASH_BIT = MAX_BITS - 8; //Bit de hash utilizado no algoritmo de busca de um match de index/prefixo nos arrays
        private const int MAX_VALUE = (1 << MAX_BITS) - 1; //Valor máximo baseado no número maximo de bits
        private const int MAX_CODE = MAX_VALUE - 1; //Código maior permitido
        private const int TABLE_SIZE = 100000; //Valor deve ser maior que o 2 elevado ao número de bits

        private int[] _categoryDictonary;
        private int[] _iaCodeTable = new int[TABLE_SIZE]; //Tabela de códigos 
        private int[] _iaPrefixTable = new int[TABLE_SIZE]; //Tabelas de Prefixos
        private int[] _iaCharTable = new int[TABLE_SIZE]; //Tabela de caracteres

        private ulong _iBitBuffer; //Buffer de bits para armazenar temporariamente os bytes de entrada do arquivo
        private int _iBitCounter; //Contador do buffer para os bits 


        public LZWClassificatorOld(int[] categoryDictonary)
        {
            _categoryDictonary = categoryDictonary;
        }

        private void Initialize() //Limpar o buffer, já que o compressor pode ser uma instância e os métodos de compressão e descompressão serem chamados da mesma
        {
            _iBitBuffer = 0;
            _iBitCounter = 0;

            for (int i = 0; i < _categoryDictonary.Length && i < TABLE_SIZE; i++)
            {
                _iaCodeTable[i] = _categoryDictonary[i];
            }
        }

        public int[] GetCodeTable() => _iaCodeTable.Where(x => x != -1).ToArray();
        public int GetCodeTableSize() => _iaCodeTable.Where(x => x != -1).Count();
        public int GetPrefixTableSize() => _iaPrefixTable.Where(x => x != 0).Count();
        public int GetCharTableSize() => _iaCharTable.Where(x => x != 0).Count();

        public bool Compress(string pInputFileName, string pOutputFileName)
        {
            Stream reader = null;
            Stream writer = null;

            try
            {
                Initialize();
                reader = new FileStream(pInputFileName, FileMode.Open);
                writer = new FileStream(pOutputFileName, FileMode.Create);
                int iChar = 0, iString = 0, iIndex = 0;

                iString = reader.ReadByte(); //Pegar primeiro caractere, 0-255 ascii char

                while ((iChar = reader.ReadByte()) != -1) //Ler o file até o final(valor -1)
                {
                    iIndex = FindMatchArray(iString, iChar); //Index correto usando o algoritmo de hash para achar match entre (prefix/code)

                    if (iIndex == -1)
                        break;

                    //Setar a string se tiver algo nesse index
                    if (_iaCodeTable[iIndex] != -1)
                    {
                        iString = _iaCodeTable[iIndex];
                        WriteCode(writer, iString);
                        iString = iChar;
                    }
                }

                WriteCode(writer, iString); //último código(ver erro do "byte ausente")
                WriteCode(writer, MAX_VALUE); //final do buffer
                WriteCode(writer, 0); //flush
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                if (writer != null)
                    writer.Close();
                File.Delete(pOutputFileName);
                return false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (writer != null)
                    writer.Close();
            }

            var counterCodeTable = GetCodeTableSize();
            var counterPrefixTable = GetPrefixTableSize();
            var counterCharTable = GetCharTableSize();

            //System.Console.WriteLine($"CodeTableSize: {counterCodeTable}, PrefixTableSize: {counterPrefixTable}, CharTableSize: {counterCharTable}");
            return true;
        }

        //hasing function, acha o index de prefix+char, senão achar retorna -1
        private int FindMatch(int pPrefix, int pChar)
        {
            int index = 0, offset = 0;

            index = (pChar << HASH_BIT) ^ pPrefix;

            offset = (index == 0) ? 1 : TABLE_SIZE - index;

            while (true)
            {
                if (_iaCodeTable[index] == -1)
                    return index;

                if (_iaPrefixTable[index] == pPrefix && _iaCharTable[index] == pChar)
                    return index;

                index -= offset;
                if (index < 0)
                    index += TABLE_SIZE;
            }
        }

        private int FindMatchArray(int pPrefix, int pChar)
        {
            int concatenatedInt = Convert.ToInt32(string.Format("{0}{1}", pPrefix, pChar));
            int index = Array.FindIndex(_iaCodeTable, x => x == concatenatedInt);

            return index;
        }

        //function para escrever os códigos em bytes para o stream através do buffer
        private void WriteCode(Stream pWriter, int pCode)
        {
            _iBitBuffer |= (ulong)pCode << (32 - MAX_BITS - _iBitCounter); //acha espaço e insere no buffer
            _iBitCounter += MAX_BITS; //incrementa counter dos bits

            while (_iBitCounter >= 8) //escreve todos os bits possíveis
            {
                int temp = (byte)((_iBitBuffer >> 24) & 255);
                pWriter.WriteByte((byte)((_iBitBuffer >> 24) & 255)); //escreve byte do buffer
                _iBitBuffer <<= 8; //remove byte escrito do buffer
                _iBitCounter -= 8; //decrementa o contador
            }
        }
    }
}
