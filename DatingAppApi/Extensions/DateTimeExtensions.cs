namespace DatingAppApi.Extensions
{
    public static class DateTimeExtensions
    {
        //public static int CalculateAge(this DateOnly dob)
        //{
        //    var today = DateOnly.FromDateTime(DateTime.UtcNow);
        //    var age = today.Year - dob.Year;
        //    if (dob > today.AddYears(-age))
        //        age--;
        //    return age;
        //}


        public static int CalculateAge(this DateTime Dob)
        {
            var today = DateTime.Today;
            var age = today.Year - Dob.Year;
            if (Dob.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }
}
