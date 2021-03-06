﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum NoteType
{
    whole = 0,
    half = 1,
    quarter = 2,
    eighth = 3
};
namespace Hack
{
    class IntervalLength
    {

        int seed = 0;

        int currentEighth = 0;
        int totalEighths = 8;

        int[] noteLength = new int[] { 8, 4, 2, 1 };
        int[] noteProbability = new int[] { 0, 5, 5, 5 };

        List<NoteType> notesInBar = new List<NoteType>();
        int[] intervalCount = new int[] { 0, 0, 0, 0 };

        // Get/set the seed
        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public List<NoteType> BarNotes(BarType barType)
        {
            // While more notes can fit into the bar
            while (currentEighth < totalEighths)
            {
                // Pick a note type, and then document its existance
                DocumentType(GetNoteType(barType));
            }
            currentEighth = 0;
            return notesInBar;
        }
        // Calculates each notes probability of being played
        void CalculateProbabilitiesNormal(BarType barType)
        {
            // If off-beat tend heavily towards eighth notes
            if (currentEighth % 2 != 0)
            {
                noteProbability[(int)NoteType.whole] = 1;
                noteProbability[(int)NoteType.half] = 3;
                noteProbability[(int)NoteType.quarter] = 6;
                noteProbability[(int)NoteType.eighth] = 40;
            }
            else
            {
                noteProbability = NoteByBar(barType);
            }

            for (int i = 0; i < 4; ++i)
            {
                // If the note length exceeds the time remaining in the bar don't allow it
                if (noteLength[i] + currentEighth > totalEighths)
                {
                    noteProbability[i] = 0;
                }
            }

        }
        // Stores note probabilities based on bar type
        int[] NoteByBar(BarType barType)
        {
            int[] probs = new int[4];
            switch (barType)
            {
                case BarType.Start:
                    probs[(int)NoteType.whole] = 6;
                    probs[(int)NoteType.half] = 10;
                    probs[(int)NoteType.quarter] = 6;
                    probs[(int)NoteType.eighth] = 2;
                    break;
                case BarType.End:
                    probs[(int)NoteType.whole] = 6;
                    probs[(int)NoteType.half] = 10;
                    probs[(int)NoteType.quarter] = 6;
                    probs[(int)NoteType.eighth] = 2;
                    break;
                case BarType.Fall:
                case BarType.Rise:
                    probs[(int)NoteType.whole] = 0;
                    probs[(int)NoteType.half] = 4;
                    probs[(int)NoteType.quarter] = 7;
                    probs[(int)NoteType.eighth] = 9;
                    break;
                case BarType.EarlyPeak:
                    if (currentEighth < totalEighths / 2)
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 4;
                        probs[(int)NoteType.eighth] = 4;
                    }
                    else
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 2;
                        probs[(int)NoteType.eighth] = 6;
                    }
                    break;
                case BarType.Peak:
                    probs[(int)NoteType.whole] = 0;
                    probs[(int)NoteType.half] = 0;
                    probs[(int)NoteType.quarter] = 4;
                    probs[(int)NoteType.eighth] = 4;
                    break;
                case BarType.LatePeak:
                    if (currentEighth < totalEighths / 2)
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 2;
                        probs[(int)NoteType.eighth] = 6;
                    }
                    else
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 4;
                        probs[(int)NoteType.eighth] = 4;
                    }
                    break;
                case BarType.EarlyTrough:
                    if (currentEighth < totalEighths / 2)
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 4;
                        probs[(int)NoteType.eighth] = 4;
                    }
                    else
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 2;
                        probs[(int)NoteType.eighth] = 6;
                    }
                    break;
                case BarType.Trough:
                    probs[(int)NoteType.whole] = 0;
                    probs[(int)NoteType.half] = 0;
                    probs[(int)NoteType.quarter] = 4;
                    probs[(int)NoteType.eighth] = 4;
                    break;
                case BarType.LateTrough:
                    if (currentEighth < totalEighths / 2)
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 2;
                        probs[(int)NoteType.eighth] = 6;
                    }
                    else
                    {
                        probs[(int)NoteType.whole] = 0;
                        probs[(int)NoteType.half] = 0;
                        probs[(int)NoteType.quarter] = 4;
                        probs[(int)NoteType.eighth] = 4;
                    }
                    break;
                default:
                    break;

            }
            return probs;
        }
        // Returns the note type to be played at this point in the bar
        NoteType GetNoteType(BarType barType)
        {
            CalculateProbabilitiesNormal(barType);
            int ratioTotal = RatioTotals();
            Random random = new Random(seed);
            float value = random.Next(0, 100);
            seed++;
            if (value <= NoteProbability(NoteType.whole, ratioTotal))
            {
                return NoteType.whole;
            }
            else if (value <= NoteProbability(NoteType.half, ratioTotal))
            {
                return NoteType.half;
            }
            else if (value <= NoteProbability(NoteType.quarter, ratioTotal))
            {
                return NoteType.quarter;
            }
            return NoteType.eighth;
        }
        // Accumulates the total value of all ratios so that each note's "stake" can be calculated
        int RatioTotals()
        {
            int total = 0;
            foreach (int prob in noteProbability)
            {
                total += prob;
            }
            return total;
        }
        // Returns the probability of this note and previous ones combined to allow for probablistic calculations
        int CumulativeProbabilities(int index)
        {
            int total = 0;
            for (int i = 0; i <= index; ++i)
            {
                total += noteProbability[i];
            }
            return total;
        }
        // Returns the float value representing a note's percentage bracket
        float NoteProbability(NoteType noteType, int ratioTotal)
        {
            return ((float)CumulativeProbabilities((int)noteType) / ratioTotal) * 100.0f;
        }
        // Adjusts data based on last note type
        void DocumentType(NoteType noteType)
        {
            notesInBar.Add(noteType);
            intervalCount[(int)noteType]++;
            currentEighth += noteLength[(int)noteType];
        }
    }
}
