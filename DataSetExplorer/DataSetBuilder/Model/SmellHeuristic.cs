﻿namespace DataSetExplorer.DataSetBuilder.Model
{
    public class SmellHeuristic
    {
        public string Description { get; }
        public bool IsApplicable { get; }
        public string ReasonForApplicability { get; }

        internal SmellHeuristic(string description, string reason)
        {
            Description = description;
            IsApplicable = true;
            ReasonForApplicability = reason;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
