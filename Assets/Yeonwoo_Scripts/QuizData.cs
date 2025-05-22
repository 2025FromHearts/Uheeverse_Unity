[System.Serializable]
public class QuizData
{
    public int id;
    public string type;          // "OX" or "MCQ"
    public string question;
    public string[] options;     // Optional for OX
    public string answer;
    public string explanation;
}
