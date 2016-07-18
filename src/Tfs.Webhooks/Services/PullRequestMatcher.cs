namespace Tfs.WebHooks.Services
{
    using Microsoft.AspNet.WebHooks.Payloads;
    using Serilog;
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class PullRequestMatcher
    {
        private readonly Regex _repositoryPattern;
        private readonly string _targetBranchName;
        private readonly string _status;

        public PullRequestMatcher(
            string repositoryPattern,
            string targetBranchName,
            string status)
        {
            _repositoryPattern = new Regex(repositoryPattern, RegexOptions.IgnoreCase);
            _targetBranchName = targetBranchName;
            _status = status;
        }

        public bool IsMatch(GitPullRequestUpdatedPayload pullRequest)
        {
            var isMatch = _repositoryPattern.IsMatch(pullRequest.Resource.Repository.Name) &&
                _status.Equals(pullRequest.Resource.Status, StringComparison.OrdinalIgnoreCase) &&
                _targetBranchName.Equals(pullRequest.Resource.TargetRefName, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                Log.Information("Pull request matching criteria ({Criteria})", this.GetCriteria());
            }
            else
            {
                Log.Information("Pull request not matching criteria - expected {ExptectedCriteria}, found {ActualValues}",
                    this.GetCriteria(),
                    this.GetPullRequestValues(pullRequest));
            }

            return isMatch;
        }

        private string GetCriteria()
        {
            var criteria = new[]
            {
                Tuple.Create("Repository pattern", _repositoryPattern.ToString()),
                Tuple.Create("Status", _status),
                Tuple.Create("Target branch name", _targetBranchName)
            };

            return string.Join(", ", criteria.Select(x => string.Format("{0}: {1}", x.Item1, x.Item2)));
        }

        private string GetPullRequestValues(GitPullRequestUpdatedPayload pullRequest)
        {
            var criteria = new[]
            {
                Tuple.Create("Repository name", pullRequest.Resource.Repository.Name),
                Tuple.Create("Status", pullRequest.Resource.Status),
                Tuple.Create("Target branch name", pullRequest.Resource.TargetRefName)
            };

            return string.Join(", ", criteria.Select(x => string.Format("{0}: {1}", x.Item1, x.Item2)));
        }
    }
}