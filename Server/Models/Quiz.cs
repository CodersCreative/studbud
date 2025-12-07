using System.Collections.Generic;
using SurrealDb.Net.Models;

namespace studbud.Shared.Models;

public class DbQuiz : Record
{
    public string? name {get; set;}
    public string? userId {get; set;}
    public string? code {get; set;}

    public DbQuiz()
    {}

    public DbQuiz(Quiz quiz)
    {
        this.name = quiz.name;
        this.userId = quiz.userId;
        this.code = quiz.code;
        if (quiz.id is not null) {
            this.Id = new RecordIdOfString("quiz", quiz.id);
        }
    }

    public Quiz ToBase()
    {
        return new Quiz {
            name = this.name,
            userId = this.userId,
            code = this.code,
            id = this.Id?.DeserializeId<string>()
        };
    }
}

public class DbQuestion : Record
{
    public string? quizId {get; set;}
    public string? text {get; set;}
    public List<string>? answers {get; set;}
    public int? correct {get; set;}

    public DbQuestion()
    {}

    public DbQuestion(Question que)
    {
        this.quizId = que.quizId;
        this.text = que.text;
        this.answers = que.answers;
        this.correct = que.correct;
        if (que.id is not null) {
            this.Id = new RecordIdOfString("question", que.id);
        }
    }

    public Question ToBase()
    {
        return new Question {
            quizId = this.quizId,
            answers = this.answers,
            text = this.text,
            correct = this.correct,
            id = this.Id?.DeserializeId<string>()
        };
    }
}