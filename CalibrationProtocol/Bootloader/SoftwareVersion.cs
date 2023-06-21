namespace CalTp.Bootloader; 

/// <summary>
/// 
/// </summary>
/// <param name="Major"></param>
/// <param name="Minor"></param>
/// <param name="Bugfix"></param>
public record SoftwareVersion(int Major, int Minor, int Bugfix);