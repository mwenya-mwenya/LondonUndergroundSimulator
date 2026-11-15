using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
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
        private IHeadwayService _headwayService;

        public FailureRuleEngine(IHeadwayService headwayService)
        {
            _headwayService = headwayService;
        }
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
            var ahead = _headwayService.GetTrainAhead(train);
            foreach (var rule in _rules)
            {
                if (rule.ShouldTrigger(dt, train, ahead ))
                    return rule;
            }

            return null;
        }
    }
}
