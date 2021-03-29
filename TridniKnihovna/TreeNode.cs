using System;
using System.Collections.Generic;
using System.Text;

namespace TridniKnihovna
{
    internal class TreeNode
    {
        public string Label { get; init; }
        
        public TreeNode [] Children { get; init; }

        internal TreeNode(string Label, TreeNode[] Children)
        {
            this.Label = Label;
            this.Children = Children;
        }
    }
}
