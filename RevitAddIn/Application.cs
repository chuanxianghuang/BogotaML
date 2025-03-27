using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;
using Nice3point.Revit.Toolkit.External;
using RevitAddIn.Commands;
using RevitAddIn.Commands.CopyOriginFilePara;
using RevitAddIn.Commands.GroupByOriginFile;
using RevitAddIn.Commands.SelectByOriginID;
using RevitAddIn.RevitFailure;

namespace RevitAddIn;

/// <summary>
///     Application entry point
/// </summary>
public class Application : ExternalApplication
{
    private const string BaseImagePath = "/RevitAddIn;component/Resources/Icons/";

    public override void OnStartup()
    {
        CreateRibbon();
        InitFailureDefintion();
    }

    //private void CreateRibbon()
    //{

    //    var toolPanel = Application.CreatePanel("EquipmentTools", "BogotaML1");

    //    var checkMepSystemBtn = toolPanel.AddPushButton<MEPSystemCheckCommand>("Equipment\n Check Integrity");
    //    checkMepSystemBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    checkMepSystemBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    checkMepSystemBtn.ToolTip = "检查系统的连接完整性。";

    //    //toolPanel.AddSeparator();
    //    var connectFillBtn = toolPanel.AddPushButton<EEEConnectCommand>("Equipment\nConnectAndFill");
    //    connectFillBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    connectFillBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    connectFillBtn.ToolTip = "连接设备，补缺或延伸。";

    //    //toolPanel.AddSeparator();
    //    var connOrDisConnBtn = toolPanel.AddPushButton<EEquipmentConnectCommand>("Equipment\n ConnectOrDisConnect");
    //    connOrDisConnBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    connOrDisConnBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    connOrDisConnBtn.ToolTip = "连接或断开设备的连接。";

    //    var originFilePanel = Application.CreatePanel("OriginTools", "BogotaML1");

    //    var copyOriginFileBtn = originFilePanel.AddPushButton<CopyOriginFileParaCommand>("Copy\nOriginFilePara");
    //    copyOriginFileBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    copyOriginFileBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    copyOriginFileBtn.ToolTip = "将构件的OriginFile参数值复制到其他构件，同时复制或统一输入OriginTag参数。";

    //    //originFilePanel.AddSeparator();
    //    var genOriginFileBtn = originFilePanel.AddPushButton<GenOriginFileParaCommand>("Gen\nOriginFilePara");
    //    genOriginFileBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    genOriginFileBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    genOriginFileBtn.ToolTip = "自动对所选构件生成OriginFile、OriginTag参数值。逻辑是查找其相连构件，提取OriginFile非空参数值。";

    //    //originFilePanel.AddSeparator();
    //    var groupByOriginFileBtn = originFilePanel.AddPushButton<GroupByOriginFileCommand>("Group\nByOriginFile");
    //    groupByOriginFileBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    groupByOriginFileBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    groupByOriginFileBtn.ToolTip = "根据不同的OriginFile参数值分别建立3D视图，并可分别导出NWC。";

    //    //originFilePanel.AddSeparator();
    //    var SelectByOriginIDBtn = originFilePanel.AddPushButton<SelectByOriginIDCommand>("Select \n ByOriginID");
    //    SelectByOriginIDBtn.SetImage("/RevitAddIn;component/Resources/Icons/MEPSystem16.png");
    //    SelectByOriginIDBtn.SetLargeImage("/RevitAddIn;component/Resources/Icons/MEPSystem32.png");
    //    SelectByOriginIDBtn.ToolTip = "输入OriginID值，选择构件。用于查找原始模型构件在合模后的位置。";
    //}
       
    private static void AddPushButton<TCommand>(RibbonPanel panel, string buttonText, string imagePath,
        string largeImagePath, string toolTip) where TCommand : class, IExternalCommand, new()
    {
        var button = panel.AddPushButton<TCommand>(buttonText);
        button.SetImage(imagePath);
        button.SetLargeImage(largeImagePath);
        button.ToolTip = toolTip;
    }

    //private void CreateRibbon()
    //{
    //    var toolPanel = Application.CreatePanel("EquipmentTools", "BogotaML1");

    //    AddPushButton<MEPSystemCheckCommand>(toolPanel, "Equipment\n Check Integrity", $"{BaseImagePath}MEPSystem16.png", $"{BaseImagePath}MEPSystem32.png", "检查系统的连接完整性。");
    //    AddPushButton<EEEConnectCommand>(toolPanel, "Equipment\nConnectAndFill", $"{BaseImagePath}MEPSystem16.png", $"{BaseImagePath}MEPSystem32.png", "连接设备，补缺或延伸。");
    //    AddPushButton<EEquipmentConnectCommand>(toolPanel, "Equipment\n ConnectOrDisConnect", $"{BaseImagePath}MEPSystem16.png", $"{BaseImagePath}MEPSystem32.png", "连接或断开设备的连接。");

    //}
    

    private static void AddPushButton<TCommand>(RibbonPanel panel, PushButtonData data) 
        where TCommand : class, IExternalCommand, new()
    {
        var button = panel.AddPushButton<TCommand>(data.ButtonText);
        button.SetImage(data.ImagePath);
        button.SetLargeImage(data.LargeImagePath);
        button.ToolTip = data.ToolTip;
    }

    private void CreateRibbon()
    {
        var toolPanel = Application.CreatePanel("EquipmentTools", "BogotaML1");

        AddPushButton<MEPSystemCheckCommand>(toolPanel, new PushButtonData
        {
            ButtonText = "Check Integrity",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "检查系统的连接完整性。"
        });

        AddPushButton<EEEConnectCommand>(toolPanel, new PushButtonData
        {
            ButtonText = "Connect & Fill",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "连接设备，补缺或延伸。"
        });

        AddPushButton<EEquipmentConnectCommand>(toolPanel, new PushButtonData
        {
            ButtonText = "Connect / Disconnect",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "连接或断开设备的连接。"
        });

        var originFilePanel = Application.CreatePanel("Model Integration & Splitting", "BogotaML1");

        AddPushButton<SelectByOriginIDCommand>(originFilePanel, new PushButtonData
        {
            ButtonText = "Select\nByOriginID",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "输入OriginID值，选择构件。用于查找原始模型构件在合模后的位置。"
        });

        AddPushButton<SelectNoOriginIDCommand>(originFilePanel, new PushButtonData
        {
            ButtonText = "Select\nNoOriginID",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "选择OriginID参数值为空的构件。可能是插件合模过程中产生，也可能是合模后调整过程中产生。"
        });

        AddPushButton<SelectNoOriginFileCommand>(originFilePanel, new PushButtonData
        {
            ButtonText = "Select\nNoOriginFile",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "选择OriginFile参数值为空的构件。用于查找合模后调整过程中新增的构件。"
        });


        AddPushButton<CopyOriginFileParaCommand>(originFilePanel, new PushButtonData
        {
            ButtonText = "Copy\nOriginFilePara",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "将构件的OriginFile参数值复制到其他构件，同时复制或统一输入OriginTag参数。"
        });

        AddPushButton<GenOriginFileParaCommand>(originFilePanel, new PushButtonData
        {
            ButtonText = "Gen\nOriginFilePara",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "自动对所选构件生成OriginFile、OriginTag参数值。逻辑是查找其相连构件，提取OriginFile非空参数值。"
        });

        AddPushButton<GroupByOriginFileCommand>(originFilePanel, new PushButtonData
        {
            ButtonText = "Group\nByOriginFile",
            ImagePath = $"{BaseImagePath}MEPSystem16.png",
            LargeImagePath = $"{BaseImagePath}MEPSystem32.png",
            ToolTip = "根据不同的OriginFile参数值分别建立3D视图，并可分别导出NWC。"
        });
    }

    private static void InitFailureDefintion()
    {
        FailureDefinition.CreateFailureDefinition(FailuresPreprocessor.CheckMEPSystemId,
             FailureSeverity.Warning, "Check MEPSystem Integrity");

    }

    public class PushButtonData
    {
        public string ButtonText { get; set; }
        public string ImagePath { get; set; }
        public string LargeImagePath { get; set; }
        public string ToolTip { get; set; }
    }
}