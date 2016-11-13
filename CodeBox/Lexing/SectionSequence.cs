﻿using System;

namespace CodeBox.Lexing
{
    public sealed class SectionSequence
    {
        private readonly string sequence;
        private readonly bool caseSensitive;
        private MatchResult lastResult;

        public SectionSequence(string sequence, bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
            this.sequence = !caseSensitive ? sequence.ToUpper() : sequence;
        }

        public MatchResult Match(char c)
        {
            if (Disabled)
                return MatchResult.Fail;

            if (c == '\t' || c == '\r' || c == '\n')
                c = ' ';

            if (lastResult == MatchResult.Hit)
                Reset();

            var sc = sequence.Length > Offset ? sequence[Offset] : '\0';
            var cc = caseSensitive ? c : char.ToUpper(c);

            var eq = sc == cc;

            if (eq && sc != ' ')
            {
                Offset++;
                MatchCount++;

                if (Offset == sequence.Length)
                    return lastResult = MatchResult.Hit;
                else
                    return lastResult = MatchResult.Proc;
            }
            else if (sc == ' ' && sequence[Offset + 1] == cc)
            {
                Offset += 2;
                MatchCount++;

                if (Offset == sequence.Length)
                    return lastResult = MatchResult.Hit;
                else
                    return lastResult = MatchResult.Proc;
            }
            else if (sc == ' ')
            {
                MatchCount++;
                return lastResult = MatchResult.Proc;
            }
            else
            {
                Reset();
                return lastResult = MatchResult.Fail;
            }
        }

        public MatchResult TryMatch(char c, int shift = 0)
        {
            var os = Offset;
            var omc = MatchCount;
            var oldret = lastResult;
            Offset += shift;
            var ret = MatchResult.Fail;

            if (Offset < sequence.Length && MatchAnyState)
                ret = MatchResult.Hit;
            else
                ret = Match(c);

            Offset = os;
            MatchCount = omc;
            lastResult = oldret;
            return ret;
        }

        public override string ToString()
        {
            return sequence;
        }

        public bool MatchAnyState => Offset < sequence.Length && sequence[Offset] == ' ';

        public char First() => sequence[0];

        public void Reset()
        {
            Offset = 0;
            MatchCount = 0;
        }

        public int Offset { get; private set; }

        public int MatchCount { get; private set; }

        public int Length => sequence.Length;

        public bool Disabled { get; set; }
    }
}
