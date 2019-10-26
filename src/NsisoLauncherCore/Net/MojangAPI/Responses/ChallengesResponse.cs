using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{

    /// <summary>
    /// 包含安全问题及其答案的ID
    /// </summary>
    public class ChallengesResponse : Response
    {
        internal ChallengesResponse(Response response) : base(response)
        {
        }

        /// <summary>
        /// 安全问题包含答案的ID，以及问题和所述问题的文本的ID。
        /// </summary>
        public struct Challenge
        {
            public long AnswerID;
            public long QuestionID;
            public string QuestionText;
        }

        public static Challenge Parse(JToken json)
        {
            JObject challenge = JObject.Parse(json.ToString());
            return new Challenge()
            {
                AnswerID = challenge["answer"]["id"].Value<long>(),
                QuestionID = challenge["question"]["id"].Value<long>(),
                QuestionText = challenge["question"]["question"].Value<string>()
            };
        }

        /// <summary>
        /// 安全问题
        /// </summary>
        public List<Challenge> Challenges { get; internal set; }
    }
}
