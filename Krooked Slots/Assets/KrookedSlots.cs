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

    string Serial, Ports;
    int Batt, Hold, Lits, Unlits, DVI, RJ, PS, RCA, PAR, SER;

    // Used as a failsafe when module cant generate, caused by SILLY edgework
    bool SolveOnSubmit = false;

    List<int> ConvertedSN36 = new List<int>();

    const string TableEquivalents = "9106574823QLAMHFUVNWIOXEGPYTRKDSBCZJ";

    List<int> SlotAUnorderedDigits = new List<int>();
    List<string> SlotAUnorderedLetters = new List<string>();
    List<int> SlotBUnorderedDigits = new List<int>();
    List<string> SlotBUnorderedLetters = new List<string> { "F", "S", "S", "F", "S", "F" };
    List<int> SlotCUnorderedDigits = new List<int>();
    List<string> SlotCUnorderedLetters = new List<string>();

    List<int> SlotAOnModel = new List<int>();
    List<int> SlotBOnModel = new List<int>();
    List<int> SlotCOnModel = new List<int>();

    List<int> SlotA_F = new List<int>();
    List<int> SlotA_S = new List<int>();
    List<int> SlotB_F = new List<int>();
    List<int> SlotB_S = new List<int>();
    List<int> SlotC_F = new List<int>();
    List<int> SlotC_S = new List<int>();

    List<int> OriginalDigits = new List<int>();
    List<int> SolutionDigits = new List<int>();

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
        Lits = Bomb.GetOnIndicators().Count();
        Unlits = Bomb.GetOffIndicators().Count();
        RJ = Bomb.GetPortCount(Port.RJ45);
        PS = Bomb.GetPortCount(Port.PS2);
        RCA = Bomb.GetPortCount(Port.StereoRCA);
        DVI = Bomb.GetPortCount(Port.DVI);
        SER = Bomb.GetPortCount(Port.Serial);
        PAR = Bomb.GetPortCount(Port.Parallel);

        ConvertSN36andSlotALettersIG();
        SlotAScramble();
        Debug.LogFormat("[Krooked Slots #{0}] The 1st slot's digits before reordering are: {1}{2}{3}{4}{5}{6}", ModuleId, SlotAUnorderedDigits[0], SlotAUnorderedDigits[1], SlotAUnorderedDigits[2], SlotAUnorderedDigits[3], SlotAUnorderedDigits[4], SlotAUnorderedDigits[5]);
        Debug.LogFormat("[Krooked Slots #{0}] The 1st slot's letters (First and Second numbers) are: {1}{2}{3}{4}{5}{6}", ModuleId, SlotAUnorderedLetters[0], SlotAUnorderedLetters[1], SlotAUnorderedLetters[2], SlotAUnorderedLetters[3], SlotAUnorderedLetters[4], SlotAUnorderedLetters[5]);
        SlotBScramble();
        Debug.LogFormat("[Krooked Slots #{0}] The 2nd slot's digits before reordering are: {1}{2}{3}{4}{5}{6}", ModuleId, SlotBUnorderedDigits[0], SlotBUnorderedDigits[1], SlotBUnorderedDigits[2], SlotBUnorderedDigits[3], SlotBUnorderedDigits[4], SlotBUnorderedDigits[5]);
        Debug.LogFormat("[Krooked Slots #{0}] The 2nd slot's letters (First and Second numbers) are: {1}{2}{3}{4}{5}{6}", ModuleId, SlotBUnorderedLetters[0], SlotBUnorderedLetters[1], SlotBUnorderedLetters[2], SlotBUnorderedLetters[3], SlotBUnorderedLetters[4], SlotBUnorderedLetters[5]);
        SlotCScramble();
        Debug.LogFormat("[Krooked Slots #{0}] The 3rd slot's digits before reordering are: {1}{2}{3}{4}{5}{6}", ModuleId, SlotCUnorderedDigits[0], SlotCUnorderedDigits[1], SlotCUnorderedDigits[2], SlotCUnorderedDigits[3], SlotCUnorderedDigits[4], SlotCUnorderedDigits[5]);
        Debug.LogFormat("[Krooked Slots #{0}] The 3rd slot's letters (First and Second numbers) are: {1}{2}{3}{4}{5}{6}", ModuleId, SlotCUnorderedLetters[0], SlotCUnorderedLetters[1], SlotCUnorderedLetters[2], SlotCUnorderedLetters[3], SlotCUnorderedLetters[4], SlotCUnorderedLetters[5]);
        GenerateWheels();
        Debug.LogFormat("[Krooked Slots #{0}] The digits displayed at the beginning of the module are: {1}{2}{3}", ModuleId, OriginalDigits[0], OriginalDigits[1], OriginalDigits[2]);
        // Add ordered list into model here!!!
        ObtainSortedSlots();
        Debug.LogFormat("[Krooked Slots #{0}] In the slots, the First digits are: {1}{2}{3} ; {4}{5}{6} ; {7}{8}{9} respectively.", ModuleId, SlotA_F[0], SlotA_F[1], SlotA_F[2], SlotB_F[0], SlotB_F[1], SlotB_F[2], SlotC_F[0], SlotC_F[1], SlotC_F[2]);
        Debug.LogFormat("[Krooked Slots #{0}] In the slots, the Second digits are: {1}{2}{3} ; {4}{5}{6} ; {7}{8}{9} respectively.", ModuleId, SlotA_S[0], SlotA_S[1], SlotA_S[2], SlotB_S[0], SlotB_S[1], SlotB_S[2], SlotC_S[0], SlotC_S[1], SlotC_S[2]);
        ObtainLotteryNumbers();
        if (SolveOnSubmit)
        {
            Debug.LogFormat("[Krooked Slots #{0}] ERROR G4M813R REACHED - Press submit to solve the module!", ModuleId);
            Debug.LogFormat("[Krooked Slots #{0}] This happened because there's at least one wheel which has no digits that appear only once, thus making it rather difficult to assign a desired position. While the manual could be tweaked to adjyst for this edgecase, I believe that it's in the spirit of a slot machine module to have a miniscule chance to solve automatically.", ModuleId);
        }
        else
        {
            Debug.LogFormat("[Krooked Slots #{0}] The goal digits are: {1}{2}{3}", ModuleId, SolutionDigits[0], SolutionDigits[1], SolutionDigits[2]);
        }
        

        // Call ResetPoint on strike..?
        ResetPoint();

        Debug.Log(SolveOnSubmit);
    }

    void ResetPoint ()
    {
        
    }

    void Update()
    { //Shit that happens at any point after initialization

    }

    void ObtainLotteryNumbers()
    {
        int dupTypeCount = 0;
        if (RJ > 1)
        {
            dupTypeCount++;
        }
        if (RCA > 1)
        {
            dupTypeCount++;
        }
        if (PS > 1)
        {
            dupTypeCount++;
        }
        if (DVI > 1)
        {
            dupTypeCount++;
        }
        if (SER > 1)
        {
            dupTypeCount++;
        }
        if (PAR > 1)
        {
            dupTypeCount++;
        }
        int helperval;
        if (OriginalDigits[0] % 2 == 0)
        {
            helperval = OriginalDigits[0] / 2;
        }
        else
        {
            helperval = (OriginalDigits[0] - 1) / 2;
        }

        // Okay what was I cooking with these
        SolutionDigits.Add((((Mathf.Min(2 * SlotA_S[2] - (SlotA_F[0] + SlotA_S[0]), 2 * SlotA_F[1] - (SlotA_F[2] + SlotA_S[1])) + helperval) % 10)+10)%10);
        SolutionDigits.Add(((((OriginalDigits[1] + 3) * (SlotB_F[1]*10+ SlotB_S[2] - (SlotB_S[1] * 10 + SlotB_F[0]) + SlotB_F[2] * 10 + SlotB_S[0]) - dupTypeCount)  % 10)+10)%10);
        SolutionDigits.Add((((-2 * OriginalDigits[2] + SlotC_S[1] - Mathf.Abs(SlotC_S[2] - SlotC_S[0]) + (SlotC_F[2] - Mathf.Abs(SlotC_F[0] - SlotC_F[1])))% 10)+10)%10);

        // BOTTOM ROUNDN TO NEAREST POSSIBLE NON TIED
        for (int i = 0; i < 15; i++)
        {
            if (SlotAUnorderedDigits.Count(x => x == SolutionDigits[0]) == 1)
            {
                break;
            }
            else
            {
                SolutionDigits[0]=(SolutionDigits[0]+9)%10;
            }
            if (i == 13)
            {
                SolveOnSubmit = true;
            }
        }
        for (int i = 0; i < 15; i++)
        {
            if (SlotBUnorderedDigits.Count(x => x == SolutionDigits[1]) == 1)
            {
                break;
            }
            else
            {
                SolutionDigits[1] = (SolutionDigits[1] + 9) % 10;
            }
            if (i == 13)
            {
                SolveOnSubmit = true;
            }
        }
        for (int i = 0; i < 15; i++)
        {
            if (SlotCUnorderedDigits.Count(x => x == SolutionDigits[2]) == 1)
            {
                break;
            }
            else
            {
                SolutionDigits[2] = (SolutionDigits[2] + 9) % 10;
            }
            if (i == 13)
            {
                SolveOnSubmit = true;
            }
        }
    }

    void ObtainSortedSlots()
    {
        int ReferenceWheelOffset = (Unlits - Lits % 3 + 3) % 3;
        int NewStartingPoint;
        // Slot A
        if (!SlotAUnorderedDigits.Contains(OriginalDigits[ReferenceWheelOffset]))
        {
            NewStartingPoint = 0;
        }
        else
        {
            NewStartingPoint = SlotAUnorderedDigits.LastIndexOf(OriginalDigits[ReferenceWheelOffset]);
        }
        Debug.LogFormat("[Krooked Slots #{0}] The 1st slot's ordered list is gotten by starting from postion {1} in it's unordered list, going forward.", ModuleId, NewStartingPoint + 1);
        for (int i = 0; i < 6; i++)
        {
            if (SlotAUnorderedLetters[(NewStartingPoint+i)%6] == "F")
            {
                SlotA_F.Add(SlotAUnorderedDigits[(NewStartingPoint + i) % 6]);
            }
            else
            {
                SlotA_S.Add(SlotAUnorderedDigits[(NewStartingPoint + i) % 6]);
            }
        }
        // B
        if (!SlotBUnorderedDigits.Contains(OriginalDigits[(1+ReferenceWheelOffset)%3]))
        {
            NewStartingPoint = 0;
        }
        else
        {
            NewStartingPoint = SlotBUnorderedDigits.LastIndexOf(OriginalDigits[(1 + ReferenceWheelOffset) % 3]);
        }
        Debug.LogFormat("[Krooked Slots #{0}] The 2nd slot's ordered list is gotten by starting from postion {1} in it's unordered list, going forward.", ModuleId, NewStartingPoint + 1);
        for (int i = 0; i < 6; i++)
        {
            if (SlotBUnorderedLetters[(NewStartingPoint + i) % 6] == "F")
            {
                SlotB_F.Add(SlotBUnorderedDigits[(NewStartingPoint + i) % 6]);
            }
            else
            {
                SlotB_S.Add(SlotBUnorderedDigits[(NewStartingPoint + i) % 6]);
            }
        }
        // C
        if (!SlotCUnorderedDigits.Contains(OriginalDigits[(2+ReferenceWheelOffset)%3]))
        {
            NewStartingPoint = 0;
        }
        else
        {
            NewStartingPoint = SlotCUnorderedDigits.LastIndexOf(OriginalDigits[(2 + ReferenceWheelOffset) % 3]);
        }
        Debug.LogFormat("[Krooked Slots #{0}] The 3rd slot's ordered list is gotten by starting from postion {1} in it's unordered list, going backward.", ModuleId, NewStartingPoint + 1);
        for (int i = 0; i < 6; i++)
        {
            if (SlotCUnorderedLetters[(NewStartingPoint + i) % 6] == "F")
            {
                SlotC_F.Add(SlotCUnorderedDigits[(NewStartingPoint + 6 - i) % 6]);
            }
            else
            {
                SlotC_S.Add(SlotCUnorderedDigits[(NewStartingPoint + 6 - i) % 6]);
            }
        }
    }

    void GenerateWheels ()
    {
        int x = Rnd.Range(0,6);
        OriginalDigits.Add(SlotAUnorderedDigits[x]);
        for (int i = 0; i < 6; i++)
        {
            SlotAOnModel.Add(SlotAUnorderedDigits[(x - i + 6) % 6]);
        }
        x = Rnd.Range(0, 6);
        OriginalDigits.Add(SlotBUnorderedDigits[x]);
        for (int i = 0; i < 6; i++)
        {
            SlotBOnModel.Add(SlotBUnorderedDigits[(x + i) % 6]);
        }
        x = Rnd.Range(0, 6);
        OriginalDigits.Add(SlotCUnorderedDigits[x]);
        for (int i = 0; i < 6; i++)
        {
            SlotCOnModel.Add(SlotCUnorderedDigits[(x + i) % 6]);
        }
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
            int y = 0;
            while (SlotAUnorderedLetters.Count(x => x == "S") > 3)
            {
                SlotAUnorderedLetters[y] = "F";
                y++;
            }
        }
        else if (SlotAUnorderedLetters.Count(x => x == "F") > 3)
        {
            int z = 0, fcount= 0;
            while (SlotAUnorderedLetters.Count(x => x == "F") > 3)
            {
                if (SlotAUnorderedLetters[z] == "F")
                {
                    fcount++;
                    if (fcount > 1)
                    {
                        SlotAUnorderedLetters[z] = "S";
                    }
                }
                z++;
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
