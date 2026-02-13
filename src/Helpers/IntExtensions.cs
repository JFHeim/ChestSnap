namespace ChestSnap.Helpers;

public static class IntExtensions
{
    public static int[] Range(this int count)
    {
        var result = new int[count];
        for (int i = 0; i < count; i++) result[i] = i;
        return result;
    }
}