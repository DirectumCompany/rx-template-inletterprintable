using GD.GovernmentSolution;
using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.PrintableTemplate.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Создание версии входящего письма с штампами рег. данных и ЭП.
    /// </summary>
    /// <param name="letter">Входящий документ.</param>
    /// <returns>Было ли запущено формирование версии.</returns>
    [Public]
    public PrintableTemplate.Structures.Module.IGeneratePrintableFormResult GeneratePrintableForm(Sungero.RecordManagement.IIncomingLetter letter)
    {
      var errors = new List<string>();
      
      if (Sungero.Company.Employees.Current == null)
        return Structures.Module.GeneratePrintableFormResult.Create(false, errors);
      
      if (string.IsNullOrEmpty(letter.InNumber))
        errors.Add(GD.PrintableTemplate.Resources.NotFilledPropertyFormat(letter.Info.Properties.InNumber.LocalizedName));
      
      if (!letter.Dated.HasValue)
        errors.Add(GD.PrintableTemplate.Resources.NotFilledPropertyFormat(letter.Info.Properties.Dated.LocalizedName));
      
      if (!letter.Versions.Where(x => Signatures.Get(x).Where(s => s.SignatureType == SignatureType.Approval && s.SignCertificate != null).Any()).Any())
        errors.Add(PrintableTemplate.Resources.NoExistsSignedVersion);
      
      if (errors.Any())
        return Structures.Module.GeneratePrintableFormResult.Create(false, errors);
      
      var dialog = Dialogs.CreateInputDialog("");
      var personalSettings = GovernmentSolution.PersonalSettings.As(Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(Sungero.Company.Employees.Current));
      
      var stampCoordinates = personalSettings.StampCoordinates.FirstOrDefault(s => string.Equals(s.Type.Value.Value, GovernmentSolution.OutgoingLetterStampCoordinates.Type.Reg.Value));
      var isStamp = stampCoordinates != null;
      var pageRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                        GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.PageNumber.LocalizedName), true, isStamp ? stampCoordinates.PageNumber : null);
      var horizontallyRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                                GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Horizontally.LocalizedName), true, isStamp ? stampCoordinates.Horizontally : null);
      var verticallyRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                              GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Vertically.LocalizedName), true, isStamp ? stampCoordinates.Vertically : null);
      var heightRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                          GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Height.LocalizedName), true, isStamp ? stampCoordinates.Height : null);
      var widthRegData = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Reg),
                                                         GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Width.LocalizedName), true, isStamp ? stampCoordinates.Width : null);
      
      stampCoordinates = personalSettings.StampCoordinates.FirstOrDefault(s => string.Equals(s.Type.Value.Value, GovernmentSolution.OutgoingLetterStampCoordinates.Type.Sig.Value));
      isStamp = stampCoordinates != null;
      var pageSignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                          GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.PageNumber.LocalizedName), true, isStamp ? stampCoordinates.PageNumber : null);
      var horizontallySignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                                  GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Horizontally.LocalizedName), true, isStamp ? stampCoordinates.Horizontally : null);
      var verticallySignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                                GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Vertically.LocalizedName), true, isStamp ? stampCoordinates.Vertically : null);
      var heightSignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                            GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Height.LocalizedName), true, isStamp ? stampCoordinates.Height : null);
      var widthSignature = dialog.AddInteger(string.Format(Resources.DialogLabelFormat, PersonalSettings.Info.Properties.StampCoordinates.Properties.Type.GetLocalizedValue(GovernmentSolution.PersonalSettingStampCoordinates.Type.Sig),
                                                           GovernmentSolution.PersonalSettings.Info.Properties.StampCoordinates.Properties.Width.LocalizedName), true, isStamp ? stampCoordinates.Width : null);
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        Functions.Module.Remote.GenerateNewVersionWithStamp(letter, pageRegData.Value.Value,
                                                            horizontallyRegData.Value.Value,
                                                            verticallyRegData.Value.Value,
                                                            heightRegData.Value.Value,
                                                            widthRegData.Value.Value,
                                                            pageSignature.Value.Value,
                                                            horizontallySignature.Value.Value,
                                                            verticallySignature.Value.Value,
                                                            heightSignature.Value.Value,
                                                            widthSignature.Value.Value);
        return Structures.Module.GeneratePrintableFormResult.Create(true, errors);
      }
      else
      {
        return Structures.Module.GeneratePrintableFormResult.Create(false, errors);
      }
    }
  }
}
