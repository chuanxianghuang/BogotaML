using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using RevitAddIn.Commands;
using RevitAddIn.RevitFailure;

namespace RevitAddIn;

/// <summary>
///     Application entry point
/// </summary>
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateRibbon();
        InitFailureDefintion();
    }

    private void CreateRibbon()
    {
       
        var panel = Application.CreatePanel("TOOLS", "BogotaML");
        panel.AddPushButton<MEPSystemCheckCommand>("检查电气设备\n连接完整性")
            .SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png")
            .SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
        panel.AddSeparator();
        panel.AddPushButton<EEEConnectCommand>("电气设备\n连接与补缺")
           .SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png")
           .SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
        panel.AddPushButton<EEquipmentConnectCommand>("电气设备\n连接或取消连接")
          .SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png")
          .SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    }

    private void InitFailureDefintion()
    {
        
       FailureDefinition.CreateFailureDefinition(FailuresPreprocessor.CheckMEPSystemId,
            FailureSeverity.Warning, "Check MEPSystem Integrity");
  
    }
}