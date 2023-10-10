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
    //Object1 => LeaveDay : Property , {'$lt': 2} : Value
    //Object2 => $lt : Property(Operator) , 2 : Value

    
    [Test]
    public async Task 案例_F001值為3_判斷其是否小於5_傳回True()
    {
        string conditionExpression = "{'LeaveDay': {'$lt': 5}}";
        string standardJson = ConvertToStandardJson(conditionExpression);
        
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "LeaveDay", 3 }
        };
        var result = EvaluateCondition(standardJson, data);
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task 案例_F001值為3_判斷其是否小2_傳回False()
    {
        string conditionExpression = "{'LeaveDay': {'$lt': 2}}"; 
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
        using (JsonDocument document = JsonDocument.Parse(conditionExpression)) 
        {
            foreach (var condition in document.RootElement.EnumerateObject())
            {
                string leaveDay = condition.Name;
                if (!data.ContainsKey(leaveDay))  //目前只考慮傳入一支表達式，處理LeaveDay相關表達式
                    return false;
                int dataValue = Convert.ToInt32(data[leaveDay]);
                
                foreach (var op in condition.Value.EnumerateObject()) 
                {
                    if (op.Name == "$lt")
                    {
                        return dataValue <　op.Value.GetInt32();
                    }
                }
            }
        }

        throw new InvalidOperationException();
    }
    
    private string ConvertToStandardJson(string input)
    {
        return input.Replace("'", "\"");
    }
}
