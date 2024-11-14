namespace QuizWebBlazor.Models
{
    public class QuestionViewModel
    {
        public string Text { get; set; }
        public List<AnswerViewModel> Answers { get; set; }

        public QuestionViewModel()
        {
            Answers = new List<AnswerViewModel>
            {
                new AnswerViewModel(),
                new AnswerViewModel(),
                new AnswerViewModel(),
                new AnswerViewModel()
            };
        }
    }
}
