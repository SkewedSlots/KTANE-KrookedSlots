using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class KrookedSlots : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;

    string Serial;
    int Batt, Hold;

    List<int> ConvertedSN36 = new List<int>();

    const string TableEquivalents = "9106574823QLAMHFUVNWIOXEGPYTRKDSBCZJ";

    List<int> SlotAUnorderedDigits = new List<int>();
    List<string> SlotAUnorderedLetters = new List<string>();
    List<int> SlotBUnorderedDigits = new List<int>();
    List<string> SlotBUnorderedLetters = new List<string> { "F", "S", "S", "F", "S", "F" };
    List<int> SlotCUnorderedDigits = new List<int>();
    List<string> SlotCUnorderedLetters = new List<string>();


    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    void OnDestroy()
    { //Shit you need to do when the bomb ends

    }

    void Activate()
    { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start()
    { //Shit that you calculate, usually a majority if not all of the module
        Serial = Bomb.GetSerialNumber().ToUpper();
        Batt = Bomb.GetBatteryCount();
        Hold = Bomb.GetBatteryHolderCount();

        ConvertSN36andSlotALettersIG();
        SlotAScramble();
        SlotBScramble();
        SlotCScramble();
        foreach (int n in SlotCUnorderedDigits)
        {
            Debug.Log(n);
        }
        foreach (string n in SlotCUnorderedLetters)
        {
            Debug.Log(n);
        }
    }

    void Update()
    { //Shit that happens at any point after initialization

    }

    void SlotCScramble()
    {
        // Use alphanumeric  for values all even pos (F), othws table (S). If batt = hold + 1, ivert letter string
        if (Batt == Hold + 1)
        {
            for (int i = 0; i < 3; i++)
            {
                SlotCUnorderedLetters.Add("F");
                SlotCUnorderedLetters.Add("S");
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                SlotCUnorderedLetters.Add("S");
                SlotCUnorderedLetters.Add("F");
            }
        }

        for (int i = 0; i < 6; i++)
        {
            if (i % 2 == 0)
            {
                SlotCUnorderedDigits.Add(CSlotTableValue(i));
            }
            else
            {
                SlotCUnorderedDigits.Add(CScrambleAlt(i));
            }
        }    
    }

    int CSlotTableValue(int loopcount)
    {
        return TableEquivalents.IndexOf(Serial[loopcount]) % 10;
    }

    int CScrambleAlt(int loopcount)
    {
        if (char.IsDigit(Serial[loopcount]))
        {
            return Serial[loopcount] - '0';
        }
        else
        {
            return (ConvertedSN36[loopcount] + 1 ) % 10;
        }
    }

    void SlotAScramble()
    {
        // change 1S to F or 2F to S
        
        if (SlotAUnorderedLetters.Count(x => x == "S") > 3)
        {
            for (int i = 0; i < SlotAUnorderedLetters.Count; i++)
            {
                if (SlotAUnorderedLetters[i] == "S")
                {
                    SlotAUnorderedLetters[i] = "F";
                    continue;
                }
            }
        }
        else if (SlotAUnorderedLetters.Count(x => x == "F") > 3)
        {
            int fcount = 0;
            for (int i = 0; i < SlotAUnorderedLetters.Count; i++)
            {
                if (SlotAUnorderedLetters[i] == "F")
                {
                    fcount++;
                    if (fcount == 2)
                    {
                        SlotAUnorderedLetters[i] = "S";
                        continue;
                    }
                }
            }
        }
    }

    void SlotBScramble()
    {
        // Pair up, base 36 % 100; format {BA}
        string pair;
        
        for (int i = 0; i < 6; i+=2)
        {
            pair = (ConvertedSN36[i] * 36 + ConvertedSN36[i+1]).ToString();
            SlotBUnorderedDigits.Add(int.Parse(pair[pair.Length - 1].ToString()));
            if (pair.Length < 2)
            {
                SlotBUnorderedDigits.Add(0);
            }
            else
            {
                SlotBUnorderedDigits.Add(int.Parse(pair[pair.Length - 2].ToString()));
            }
        }
    }

    void ConvertSN36andSlotALettersIG()
    {
        // Getbase36Sn && SLOTA: Digits normal(S); Letters + 9(F)
        foreach (char c in Serial)
        {
            if (char.IsDigit(c))
            {
                ConvertedSN36.Add(c - '0');
                SlotAUnorderedDigits.Add(c - '0');
                SlotAUnorderedLetters.Add("S");
            }
            else
            {
                ConvertedSN36.Add(c - 'A' + 10);
                SlotAUnorderedLetters.Add("F");
                SlotAUnorderedDigits.Add((c - 'A') % 10);
            }
        }
    }

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }
    /* Delete this if you dont want TP integration
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }*/
}
