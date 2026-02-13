namespace ChestSnap.Helpers;

public static class VectorExtensions
{
    public static Vector2 Round(this Vector2 vector, int n) =>
        new Vector2(
            (float)Math.Round(vector.x, n),
            (float)Math.Round(vector.y, n));

    public static Vector3 Round(this Vector3 vector, int n) =>
        new Vector3(
            (float)Math.Round(vector.x, n),
            (float)Math.Round(vector.y, n),
            (float)Math.Round(vector.z, n));
}