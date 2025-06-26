public class SentimentAnalyzer
{
    private readonly Dictionary<string, string> _sentimentKeywords = new()
    {
        ["worried"] = "worried|concerned|nervous|anxious|scared|afraid|fear|unsure|what if",
        ["frustrated"] = "frustrated|angry|mad|annoyed|pissed|irritated|fed up|sick of",
        ["curious"] = "curious|interested|wonder|how does|what is|why does|explain|tell me",
        ["confused"] = "confused|don't understand|not sure|what do you mean|help me",
        ["excited"] = "excited|happy|great|awesome|thank you|thanks|appreciate|love it"
    };

    public string DetectSentiment(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "neutral";

        input = input.ToLower();

        foreach (var sentiment in _sentimentKeywords)
        {
            if (sentiment.Value.Split('|').Any(keyword => input.Contains(keyword)))
            {
                return sentiment.Key;
            }
        }

        return "neutral";
    }

    public string GetResponseAdjustment(string sentiment, string input)
    {
        input = input.ToLower();

        if (sentiment == "frustrated")
        {
            if (input.Contains("password"))
            {
                return "I understand passwords can be frustrating! Here's the good news - ";
            }
            else if (input.Contains("phishing") || input.Contains("scam"))
            {
                return "Dealing with phishing attempts is annoying, but ";
            }
            else if (input.Contains("hack") || input.Contains("breach"))
            {
                return "Security breaches are definitely frustrating. Remember that ";
            }

            return "I hear your frustration. Let me help - ";
        }

        return sentiment switch
        {
            "worried" => "I understand this can be concerning. Let me reassure you that ",
            "curious" => "That's a great question! Here's what you should know: ",
            "confused" => "I'd be happy to clarify this for you. Essentially, ",
            "excited" => "That's wonderful to hear! I'm excited to share that ",
            _ => ""
        };
    }

    public string GetTopicSpecificResponse(string input)
    {
        input = input.ToLower();

        if (input.Contains("password"))
        {
            return "Passwords don't have to be frustrating! Try using passphrases - " +
                   "combine 3-4 random words with a number and symbol (like 'PurpleTiger$42'). " +
                   "Password managers can also help by remembering them for you!";
        }
        else if (input.Contains("phishing"))
        {
            return "Phishing emails are annoying but easy to spot once you know the signs. " +
                   "Always check sender addresses carefully and hover over links before clicking.";
        }

        return "";
    }
}