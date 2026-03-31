using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System.Collections.Generic;

namespace LondonUndergroundSimulator.Services
{
    /// <summary>
    /// Evaluates failure rules and returns the first triggered rule.
    /// Contains NO side effects. Does NOT modify trains.
    /// </summary>
    public class FailureRuleEngine : IFailureRuleEngine
    {
        private readonly List<IFailureRule> _rules = new();

        public void AddRule(IFailureRule rule)
        {
            _rules.Add(rule);
        }

        /// <summary>
        /// Evaluates all rules for a train.
        /// Returns the triggered rule, or null if none triggered.
        /// </summary>
        public IFailureRule? Evaluate(Train train, float dt)
        {
            foreach (var rule in _rules)
            {
                if (rule.ShouldTrigger(dt))
                    return rule;
            }

            return null;
        }
    }
}
