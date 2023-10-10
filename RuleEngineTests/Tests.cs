using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Text.Json;
using InvalidOperationException = System.InvalidOperationException;

namespace RuleEngineTests;

//表達式範例
//conditionExpression = "{'LeaveDay': {'$lt': 5}}"
//Object1 : Condition(e.g 請假、公出...etc) =>  Property: LeaveDay( 各種情境 e.g. 'F0001'...etc) , Value: {'$lt': 2}
//Object2 : Operator (e.g. $eq, $lt)      =>  Property: $lte , Value(Threshold): 2

public class ConditionExpressionTest
{
    //可視需求擴增新增運算子、參數型別
    //目前假設情境為請假，傳入參數為請假天數 ，測試型別皆為Int
    
    private readonly Dictionary<string, Func<int, int, bool>> _operators = new()
    {
        { "$lte", (dataValue, threshold) => dataValue <= threshold},
        { "$lt", (dataValue, threshold) => dataValue < threshold },
        { "$eq", (dataValue, threshold) => dataValue == threshold },
        { "$gt", (dataValue, threshold) => dataValue > threshold },
        { "$gte",(dataValue, threshold) => dataValue >= threshold },
        { "$ne", (dataValue, threshold) => dataValue != threshold }
    };
    
    [SetUp]
    public Task Setup()
    {
        return Task.CompletedTask;
    }


    #region Comparison Query Operators
    [Test]
    public Task 案例_請假天數為3天_是否小於等於5天_傳回True()
    {
        string conditionExpression = ConvertToStandardJson("{'LeaveDay': {'$lte': 5}}");
        
        Dictionary<string, object> data = new()
        {
            { "LeaveDay", 3 }
        };
        
        var result = EvaluateCondition(conditionExpression, data);
        result.Should().BeTrue();
        return Task.CompletedTask;
    }
    
    [Test]
    public Task 案例_請假天數為3天_是否小於等於2天_傳回False()
    {
        string conditionExpression = ConvertToStandardJson("{'LeaveDay': {'$lte': 2}}"); 
        
        Dictionary<string, object> data = new()
        {
            { "LeaveDay", 3 }
        };
        var result = EvaluateCondition(conditionExpression, data);
        result.Should().BeFalse();
        return Task.CompletedTask;
    }
    
    [Test]
    public Task 案例_請假天數為6天_是否大於5天_傳回True()
    {
        string conditionExpression = ConvertToStandardJson("{'LeaveDay': {'$gt': 5}}");
    
        Dictionary<string, object> data = new()
        {
            { "LeaveDay", 6 }
        };
        
        var result = EvaluateCondition(conditionExpression, data);
        result.Should().BeTrue();
        return Task.CompletedTask;
    }
    
    [Test]
    public Task 案例_請假天數為6天_是否等於6天_傳回True()
    {
        string conditionExpression = ConvertToStandardJson("{'LeaveDay': {'$eq': 6}}");
    
        Dictionary<string, object> data = new()
        {
            { "LeaveDay", 6 }
        };
        
        var result = EvaluateCondition(conditionExpression, data);
        result.Should().BeTrue();
        return Task.CompletedTask;
    }
    #endregion

    #region Logical Query Operators

    [Test]
    public Task 案例_請假天數為3天_是否小於等於5天且有請假權限()
    {
        //Todo => $and
        return Task.CompletedTask;
    }

    #endregion
    
    #region Rule Pattern

    [Test]
    public Task 案例_如果情境為請假單_執行EvaluateCondition_如果情境為公出單_執行EvaluateCondition2()
    {
        //Todo
        return Task.CompletedTask;
    }

    #endregion
    
    private bool EvaluateCondition(string conditionExpression, Dictionary<string, object> data)
    {
        using JsonDocument conditionJson = JsonDocument.Parse(conditionExpression);
        foreach (var condition in conditionJson.RootElement.EnumerateObject())
        {
            foreach (var op in condition.Value.EnumerateObject())
            {
                if (_operators.ContainsKey(op.Name))
                {
                    string operatorType = op.Name;
                    int dataValue = (int)data[condition.Name];
                    int threshold = op.Value.GetInt32();
                    return _operators[operatorType](dataValue, threshold);
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
