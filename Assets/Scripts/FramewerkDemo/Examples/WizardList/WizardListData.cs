namespace FramewerkDemo.Examples.WizardList
{
    public class WizardListData
    {
        public string Title { get; private set; }
        public int Value { get; private set; }

        public WizardListData(string title, int value)
        {
            Title = title;
            Value = value;
        }
    }
}
