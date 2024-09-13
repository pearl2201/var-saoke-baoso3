namespace SaokeApp
{
    public class Helpers
    {
        public static string RenderMoney(long amount)
        {
            if (amount < 1000000)
            {
                return (amount / 1000).ToString("#.##K");
            }
            else if (amount < 1000000000)
            {
                return (amount / 1000000).ToString("#.## triệu");
            }
            else
            {
                return (amount / 1000000000).ToString("#.## tỷ");
            }
        }
    }
}
