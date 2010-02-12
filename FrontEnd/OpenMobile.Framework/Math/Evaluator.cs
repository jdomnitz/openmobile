/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Collections.Generic;

namespace OpenMobile.Framework.Math
{
    using Math = System.Math;
    class Evaluator
    {
        public int steps;
        public string Evaluate(string expression)
        {
            steps = 0;
            expression = cleanUp(expression);
            try
            {
                return evaluateStep(expression).ToString();
            }
            catch (Exception)
            {
                int count = 0;
                for (int i = 0; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        count++;
                    if (expression[i] == ')')
                        count--;
                }
                if (count != 0)
                    return "Invalid Formula-Open Parenthesis";
                return "Invalid Formula";
            }
        }

        private double evaluateStep(string expression)
        {
            steps++;
            if (expression.Contains("(") == false)
                return Convert.ToDouble(eval(expression));
            return Convert.ToDouble(evaluateStep(breakitup(expression)));
        }

        private string breakitup(string expression)
        {
            string prefix="";
            string inside="";
            if (expression[0] != '(')
                prefix = expression.Substring(0, expression.IndexOf('('));
            inside=findInside(expression.Substring(prefix.Length+1));
            return prefix+ evaluateStep(inside)+expression.Substring(prefix.Length+inside.Length+2);
        }

        private string findInside(string p)
        {
            int index = 1;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == '(')
                    index++;
                if (p[i] == ')')
                    index--;
                if (index == 0)
                    return p.Remove(i);
            }
            return p;
        }

        private string cleanUp(string expression)
        {
            expression = expression.ToUpper().Replace(" ", "");
            expression = expression.Replace("PI", Math.PI.ToString());
            expression = expression.Replace("E", Math.E.ToString());
            //Convert functions to operators
            expression = expression.Replace("SIN", "S").Replace("COS", "C").Replace("TAN", "T").Replace("SEC", "F");
            expression = expression.Replace("CSC", "G").Replace("COT", "H").Replace("LOG", "D").Replace("LN", "E");
            return expression.Replace("ABS", "A").Replace("SQRT", "B").Replace("SIGN", "I").Replace("<<","L").Replace(">>","R");
        }

        //Is the character an operator
        private bool isOp(char c)
        {
            switch (c)
            {
                case '+':  //Addition
                    return true;
                case '-': //Subtraction
                    return true;
                case '*': //Multiplication
                    return true;
                case '/': //Division
                    return true;
                case '^': //Exponent
                    return true;
                default:
                    return false;
            }
        }

        private string eval(string s)
        {
            //If no operation is needed - don't attempt one
            if (s.Length == 1)
                return s;
            //Take care of minus signs
            if (s[0] == '-')
                s = '~'+s.Substring(1);
            s = s.Replace("*-", "*~").Replace("--", "+").Replace("/-", "/~").Replace("+-", "+~").Replace("^-", "^~");
            //Split the string into numbers and operations
            List<string> args=new List<string>(s.Split(new char[]{'+','-','*','/','^'},StringSplitOptions.RemoveEmptyEntries));
            List<char> op = new List<char>();
            foreach (char c in s)
            {
                if (isOp(c) == true)
                    op.Add(c);
            }
            //If it starts or ends with an operation - it operates on parenthesis so come back to it
            if (op.Count > 0)
            {
                if (s[0] == op[0])
                {
                    return s;
                }
                if (s[s.Length - 1] == op[op.Count - 1])
                {
                    return s;
                }
            }
            //unescape the minus signs
            for (int i = 0; i < args.Count; i++)
            {
                args[i] = args[i].Replace('~', '-');
            }
            
            //Solve any functions that can be
            for (int i = 0; i < args.Count; i++)
            {
                switch (args[i][0])
                {
                    case 'A': //Absolute Value
                        args[i] = Math.Abs(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                    case 'B': //Sqrt()
                        args[i] = Math.Sqrt(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                    case 'C': //Cos()
                        args[i] = Math.Cos(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                    case 'D': //Log()
                        args[i] = Math.Log(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                    case 'E': //Natural Log - Ln()
                        args[i] = Math.Log(Convert.ToDouble(args[i].Substring(1)),Math.E).ToString();
                        break;
                    case 'F': //Sec
                        args[i] = (1/Math.Cos(Convert.ToDouble(args[i].Substring(1)))).ToString();
                        break;
                    case 'G': //CSC()
                        args[i] = (1/Math.Sin(Convert.ToDouble(args[i].Substring(1)))).ToString();
                        break;
                    case 'H': //COT()
                        args[i] = (1/Math.Tan(Convert.ToDouble(args[i].Substring(1)))).ToString();
                        break;
                    case 'I': //The sign of the number
                        args[i] = Math.Sign(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                    case 'S': //SIN()
                        args[i] = Math.Sin(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                    case 'T': //TAN()
                        args[i] = Math.Tan(Convert.ToDouble(args[i].Substring(1))).ToString();
                        break;
                }
                if (args[i].Contains("L") == true)
                { //Bit Shift Left
                    string[] parts=args[i].Split(new char[]{'L'});
                    args[i] = (Convert.ToInt32(parts[0]) << Convert.ToInt32(parts[1])).ToString();
                }
                if (args[i].Contains("R") == true)
                { //Bit Shift Right
                    string[] parts = args[i].Split(new char[] { 'R' });
                    args[i] = (Convert.ToInt32(parts[0]) >> Convert.ToInt32(parts[1])).ToString();
                }
            }

            //Order of operations says to do exponents first
            for (int i = 0; i < op.Count; i++)
            {
                if (op[i]=='^')
                {
                    args[i] = Math.Pow(Convert.ToDouble(args[i]), Convert.ToDouble(args[i + 1])).ToString();
                    args.RemoveAt(i + 1);
                    op.RemoveAt(i);
                    i = i - 1;
                }
            }

            //Followed by multiplication and division
            for (int i = 0; i < op.Count;i++)
            {
                if (op[i]== '*')
                {
                    args[i] = (Convert.ToDouble(args[i]) * Convert.ToDouble(args[i + 1])).ToString();
                    args.RemoveAt(i + 1);
                    op.RemoveAt(i);
                    i = i - 1;
                }
                else if (op[i]=='/')
                {
                    args[i] = (Convert.ToDouble(args[i]) / Convert.ToDouble(args[i + 1])).ToString();
                    args.RemoveAt(i + 1);
                    op.RemoveAt(i);
                    i = i - 1;
                }
            }
            //Followed by addition or subtraction
            for (int i = 0; i < op.Count; i++)
            {
                if (op[i]== '+'){
                    args[i] = (Convert.ToDouble(args[i]) + Convert.ToDouble(args[i + 1])).ToString();
                    args.RemoveAt(i + 1);
                    op.RemoveAt(i);
                    i = i - 1;
                }
                else if(op[i]== '-')
                {
                    args[i] = (Convert.ToDouble(args[i]) - Convert.ToDouble(args[i + 1])).ToString();
                    args.RemoveAt(i + 1);
                    op.RemoveAt(i);
                    i = i - 1;
                }
            }
                return ' ' + args[0] + ' ';
        }
    }
}
