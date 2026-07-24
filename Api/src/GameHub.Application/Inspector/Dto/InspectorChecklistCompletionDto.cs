namespace GameHub.Inspector.Dto
{
    public class InspectorChecklistCompletionDto
    {
        public int TotalQuestions { get; set; }

        public int AnsweredQuestions { get; set; }

        public double CompletionPercentage { get; set; }
    }
}
