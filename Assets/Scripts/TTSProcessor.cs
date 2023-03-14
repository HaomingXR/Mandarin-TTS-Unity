using UnityEngine;
using System.Collections.Generic;

public class TTSProcessor
{
    /// <summary>
    /// Perform preprocessing for Baker dataset
    /// </summary>
    /// <param name="text">Input Text</param>
    /// <returns>Array of Integer IDs</returns>
    public static int[] TextToSequence(ref string text)
    {
        List<string> pinyin = Baker.Parse(ref text);
        List<string> newPinyin = new List<string>();

        foreach (string x in pinyin)
        {
            string phoneme = string.Join("", x);
            if (!phoneme.Contains("#"))
                newPinyin.Add(phoneme);
        }

        List<string> phonemes = Baker.GetPhonemeFromCharAndPinyin(text, newPinyin);

        List<int> sequence = new List<int>();
        Debug.Log(string.Join(", ", phonemes));
        foreach (string symbol in phonemes)
        {
            try
            {
                int id = Baker.Symbol_to_ID[symbol];
                sequence.Add(id);
            }
            catch
            {
                Debug.Log($"Fuck: {symbol}");
            }
        }

        sequence.Add(Baker.Symbol_to_ID["eos"]);
        return sequence.ToArray();
    }
}