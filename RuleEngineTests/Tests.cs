using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Text.Json;

namespace RuleEngineTests;

public class ConditionExpressionTest
{
    [SetUp]
    public async Task Setup()
    {
    }

    //conditionExpression = "{'LeaveDay': {'$lt': 5}}"
    //Object1 : Condition(e.g 請假、公出...etc) =>  Property: LeaveDay , Value: {'$lt': 2}
    //Object2 : Operator (e.g. $eq, $lt)      =>  Property: $lte ,     Value(Threshold): 2

    
    [Test]
    public async Task 案例_請假天數為3天_判斷其是否小於等於5天_傳回True()
    {
        string conditionExpression = "{'LeaveDay': {'$lte': 5}}";
        string standardJson = ConvertToStandardJson(conditionExpression);
        
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "LeaveDay", 3 }
        };
        var result = EvaluateCondition(standardJson, data);
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task 案例_請假天數為3天_判斷其是否小於等於2天_傳回False()
    {
        string conditionExpression = "{'LeaveDay': {'$lte': 2}}"; 
        string standardJson = ConvertToStandardJson(conditionExpression);
        
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "LeaveDay", 3 }
        };
        var result = EvaluateCondition(standardJson, data);
        result.Should().BeFalse();
    }

    
    private bool EvaluateCondition(string conditionExpression, Dictionary<string, object> data)
    {
        using JsonDocument document = JsonDocument.Parse(conditionExpression);
        foreach (var condition in document.RootElement.EnumerateObject())
        {
            if (!GetValueIfConditionMatches(data, condition, out var threshold)) break; 
         
            //目前假設情境為請假，傳入參數為請假天數 ，測試型別皆為Int
            var dataValue = Convert.ToInt32(threshold);
            foreach (var op in condition.Value.EnumerateObject()) 
            {
                if (op.Name == "$lte")
                {
                    return dataValue <=　op.Value.GetInt32();
                }
            }
        }

        throw new InvalidOperationException();
    }

    private static bool GetValueIfConditionMatches(Dictionary<string, object> data, JsonProperty condition, out object threshold)
    {
        return data.TryGetValue(condition.Name, out threshold);
    }

    private string ConvertToStandardJson(string input)
    {
        return input.Replace("'", "\"");
    }
}
