using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    internal class RegularExpressionParser
    {
        private string Pattern { get; init; }

        private int position;

        public RegularExpressionParser(string RegularExpression)
        {
            this.Pattern = RegularExpression;

            this.position = 0;
        }

        public TreeNode toParseTree()
        {
            return Expression();
        }
        private char peek()
        {
            return this.Pattern[this.position];
        }

        private bool hasMoreChars()
        {
            return this.position < this.Pattern.Length;
        }

        private bool isMetaChar(char ch)
        {
            return ch.Equals('*') || ch.Equals('+') || ch.Equals('?');
        }

        private bool Match(char ch)
        {
            if (peek().Equals(ch))
            {
                this.position++;

                return true;
            }
            else
            {
                Console.WriteLine("Unexpected symbol: " + ch);

                return false;
            }
        }

        private char Next()
        {
            char ch = peek();
            Match(ch);

            return ch;
        }

        private TreeNode Expression()
        {
            TreeNode trm = Term();

            if (hasMoreChars() && peek() == '|')
            {
                Match('|');
                TreeNode exp = Expression();

                return new TreeNode("Expression", new TreeNode[] { trm, new TreeNode("|", null), exp });
            }

            return new TreeNode("Expression", new TreeNode[] { trm });
        }

        private TreeNode Term()
        {
            TreeNode factr = Factor();

            if (hasMoreChars() && !peek().Equals(')') && !peek().Equals('|'))
            {
                TreeNode trm = Term();

                return new TreeNode("Term", new TreeNode[] { factr, trm });
            }

            return new TreeNode("Term", new TreeNode[] { factr });
        }

        private TreeNode Factor()
        {
            TreeNode atm = Atom();

            if (hasMoreChars() && isMetaChar(peek()))
            {
                char meta = Next();

                return new TreeNode("Factor", new TreeNode[] { atm, new TreeNode(meta.ToString(), null) });
            }

            return new TreeNode("Factor", new TreeNode[] { atm });
        }

        private TreeNode Atom()
        {
            if (peek().Equals('('))
            {
                Match('(');
                TreeNode exp = Expression();
                Match(')');

                return new TreeNode("Atom", new TreeNode[] { new TreeNode("(", null), exp, new TreeNode(")", null) });
            }

            TreeNode ch = Char();

            return new TreeNode("Atom", new TreeNode[]{ ch });
        }

        private TreeNode Char()
        {
            if (isMetaChar(peek()))
            {
                Console.WriteLine("Unexpected meta char: " + peek());

                return null;
            }

            if (peek().Equals('\\'))
            {
                Match('\\');

                return new TreeNode("Char", new TreeNode[] { new TreeNode('\\'.ToString(), null),
                    new TreeNode(Next().ToString(), null) });
            }

            return new TreeNode("Char", new TreeNode[] { new TreeNode(Next().ToString(), null) });
        }
    }
}
