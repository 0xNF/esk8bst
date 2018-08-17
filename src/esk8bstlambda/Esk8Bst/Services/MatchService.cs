using Esk8Bst.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Esk8Bst.Services
{
    public class MatchService
    {

        private readonly ILogger Logger;

        public MatchService(ILogger logger) {
            this.Logger = logger;
        }

        public Dictionary<Subscriber, List<LambdaMatch>> MakeMatches(IEnumerable<Subscriber> subscribers, IEnumerable<BSTComment> comments) {
            Dictionary<Subscriber, List<LambdaMatch>> lambdadelta = new Dictionary<Subscriber, List<LambdaMatch>>();
            foreach (Subscriber s in subscribers) {
                List<LambdaMatch> lambdamatches = new List<LambdaMatch>();
                foreach (Match m in s.Matches) {
                    LambdaMatch lm = new LambdaMatch(m);
                    foreach (BSTComment comment in comments) {
                        // BST type matched
                        if (comment.BuySellTradeStatus == m.BST || m.BST == BST.BST) { // if match bst is BST, match every post
                            // Company type matches
                            if (m.Companies.Contains(comment.Company.ToLowerInvariant()) || m.Companies.Count == 0) { // If companies array is empyty, match every company
                                if ((m.BST == BST.BUY || m.BST == BST.BST) && (!m.Price.HasValue || comment.Price >= m.Price)) { // If price is null, match any price
                                    // MATCH FOUND
                                    lm.Posts.Add(comment);
                                }
                                else if ((m.BST == BST.SELL || m.BST == BST.BST) && (!m.Price.HasValue || comment.Price <= m.Price)) {
                                    // MATCH FOUND
                                    lm.Posts.Add(comment);
                                }
                                else if ((m.BST == BST.TRADE || m.BST == BST.BST)) {
                                    // MATCH FOUND
                                    lm.Posts.Add(comment);
                                }
                            }
                        }
                    }
                    if (lm.Posts.Any()) {
                        lambdamatches.Add(lm);
                    }
                }
                if (lambdamatches.Any()) {
                    lambdadelta.Add(s, lambdamatches);
                }
            }
            return lambdadelta;
        }
    }
}
