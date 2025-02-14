public interface IJoystickInput
{
    bool DebugMode { get; set; }
    void Update(float deltaTime);
    public int Value { get; }
    public string[] Symbols { get; }
}
