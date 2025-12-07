using System.Collections.Generic;

namespace studbud.Shared.Models;

public class Quiz
{
    public string? name {get; set;}
    public string? userId {get; set;}
    public string? code {get; set;}
    public string? id {get; set;}
    public Quiz()
    {}
}

public class Question
{
    public string? quizId {get; set;}
    public string? text {get; set;}
    public List<string>? answers {get; set;}
    public int? correct {get; set;}
    public string? id {get; set;}
    public Question()
    {}
}