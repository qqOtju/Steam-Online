public static class StringExtension
{
    public static string SceneName(this string sceneName)
    {
        sceneName = sceneName.Remove(0, sceneName.LastIndexOf('/') + 1);
        sceneName = sceneName.Remove(sceneName.IndexOf('.'));
        return sceneName;
    }
}
