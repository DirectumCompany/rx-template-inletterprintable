using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.PrintableTemplate.Server
{
  public partial class ModuleFunctions
  {
    /// <summary>
    /// Создать версию в формате pdf с штампами.
    /// </summary>
    /// <param name="letter">Входящий документ.</param>
    /// <param name="pageRegData">Номер страницы для простановки штампа рег. данных.</param>
    /// <param name="horizontallyRegData">Горизонтальный отступ для простановки штампа рег. данных.</param>
    /// <param name="verticallyRegData">Вертикальный отступ для простановки штампа рег. данных.</param>
    /// <param name="heightRegData">Высота штампа рег. данных.</param>
    /// <param name="widthRegData">Ширина штампа рег. данных.</param>
    /// <param name="pageSignature">Номер страницы для простановки штампа ЭП.</param>
    /// <param name="horizontallySignature">Горизонтальный отступ для простановки штампа ЭП.</param>
    /// <param name="verticallySignature">Вертикальный отступ для простановки штампа ЭП.</param>
    /// <param name="heightSignature">Высота штампа ЭП.</param>
    /// <param name="widthSignature">Ширина штампа ЭП.</param>
    [Remote]
    public void GenerateNewVersionWithStamp(Sungero.RecordManagement.IIncomingLetter letter,
                                            int pageRegData, int horizontallyRegData, int verticallyRegData, int heightRegData, int widthRegData,
                                            int pageSignature, int horizontallySignature, int verticallySignature, int heightSignature, int widthSignature)
    {
      var version = letter.Versions.Where(x => Signatures.Get(x).Where(s => s.SignatureType == SignatureType.Approval).Any()).OrderByDescending(x => x.Id).FirstOrDefault();
      if (version == null)
        return;
      var signature = Signatures.Get(version)
        .Where(s => s.SignatureType == SignatureType.Approval && s.SignCertificate != null).FirstOrDefault();
      if (signature == null)
        return;
        
      var certificateInfo = Sungero.Docflow.PublicFunctions.Module.GetSignatureCertificateInfo(signature.GetDataSignature());
      var signatureSerialNumber = certificateInfo != null && certificateInfo.Serial != null ? certificateInfo.Serial : string.Empty;
      var signatureUserName = signature.SignatoryFullName != null ? signature.SignatoryFullName : string.Empty;
      var signatureDates = string.Format("с {0} по {1}", signature.SignCertificate.NotBefore.Value.ToString("dd.MM.yyyy"), signature.SignCertificate.NotAfter.Value.ToString("dd.MM.yyyy"));
      
      var stampCoordinatesList = new List<GD.MEDO.Structures.Module.IStampCoordinates>();
      stampCoordinatesList.Add(GD.MEDO.Structures.Module.StampCoordinates.Create(GD.GovernmentSolution.OutgoingLetterStampCoordinates.Type.Sig.Value,
                                                                                 pageSignature,
                                                                                 horizontallySignature,
                                                                                 verticallySignature,
                                                                                 heightSignature,
                                                                                 widthSignature,
                                                                                 null,
                                                                                 null,
                                                                                 new int[] { pageSignature },
                                                                                 signature.Id,
                                                                                 signatureSerialNumber,
                                                                                 signatureUserName,
                                                                                 signature.SignatureType.ToString(),
                                                                                 signatureDates,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null));
      
      stampCoordinatesList.Add(GD.MEDO.Structures.Module.StampCoordinates.Create(GD.GovernmentSolution.OutgoingLetterStampCoordinates.Type.Reg.Value,
                                                                                 pageRegData,
                                                                                 horizontallyRegData,
                                                                                 verticallyRegData,
                                                                                 heightRegData,
                                                                                 widthRegData,
                                                                                 null,
                                                                                 this.GetRegStamp(letter),
                                                                                 new int[] { pageRegData },
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null,
                                                                                 null));

      var sigStampCoordinates = stampCoordinatesList.FirstOrDefault(x => Equals(x.Type, GD.GovernmentSolution.OutgoingLetterStampCoordinates.Type.Sig.Value));
      if (sigStampCoordinates != null)
        sigStampCoordinates.HtmlStamp = MEDO.PublicFunctions.Module.GetSignatureStamp(letter, sigStampCoordinates);
      
      var newVersion = this.CreatePdfVersionFromVersion(letter, version);
      using (var pdfDocumentStream = new MemoryStream())
      {
        newVersion.Body.Read().CopyTo(pdfDocumentStream);
        using (var stampedIsolatedStream = MEDO.IsolatedFunctions.StampTools.AddStampsToPdf(pdfDocumentStream, stampCoordinatesList))
        {
          newVersion.Body.Write(stampedIsolatedStream);
        }
      }
      letter.Save();
    }
     
    /// <summary>
    /// Сформировать текст для штампа рег данных.
    /// </summary>
    /// <param name="document">Письмо.</param>
    /// <returns>Штамп рег. данных в формате HTML.</returns>
    [Public]
    public virtual string GetRegStamp(Sungero.RecordManagement.IIncomingLetter document)
    {
      var htmlTemplate = this.GetRegistrationStampHtmlTemplate();
      var stampSettings = GD.GovernmentSolution.PublicFunctions.StampSetting.GetStampSetting(document);
      var templateRegDataMainDoc = stampSettings.TemplateRegDataMainDocGD;
      var templateRegDataInDoc = stampSettings.TemplateRegDataInDocGD;

      var stampLines = new List<string>();
      var regDate = document.RegistrationDate != null ? document.RegistrationDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      var regNum = document.RegistrationNumber;
      var regInfoStamp = string.Empty;
      if (string.IsNullOrEmpty(templateRegDataMainDoc))
        regInfoStamp = string.Format(MEDO.Resources.RegInfoStampTemplate, regDate, document.RegistrationNumber);
      else
      {
        regInfoStamp = templateRegDataMainDoc.Replace("{RegNum}", regNum);
        regInfoStamp = regInfoStamp.Replace("{RegDate}", regDate);
      }
      stampLines.Add(regInfoStamp);
      
      // Добавить в штамп информацию об исходящем письме.
      if (!string.IsNullOrEmpty(templateRegDataInDoc) && document.Dated != null && document.InNumber != null)
      {
        var incomingDocDate = string.Empty;
        var incomingRegNumber = string.Empty;
        incomingDocDate = document.Dated.Value.ToString("dd.MM.yyyy");
        incomingRegNumber = document.InNumber;

        var linkedDocumentInfoStamp = templateRegDataInDoc.Replace("{InNum}", incomingRegNumber);
        linkedDocumentInfoStamp = linkedDocumentInfoStamp.Replace("{InDate}", incomingDocDate);
        stampLines.Add(linkedDocumentInfoStamp);
      }
      
      var stampText = string.Join("\n", stampLines);
      // Убрать из строки html-теги.
      stampText = System.Text.RegularExpressions.Regex.Replace(stampText, @"<[^>]+>", string.Empty);
      // Заменить переносы строк на теги <br>.
      stampText = stampText.Replace("\n", "<br>");
      return htmlTemplate.Replace("{RegNum}", stampText);
    }
    
    /// <summary>
    /// Получить html-шаблон штампа рег. данных для документа.
    /// </summary>
    /// <returns>Html-шаблон штампа рег. данных.</returns>
    public virtual string GetRegistrationStampHtmlTemplate()
    {
      return GD.MEDO.Resources.RegistrationStampHtmlTemplate;
    }
    
    /// <summary>
    /// Создание pdf версии документа.
    /// </summary>
    /// <param name="document">Входящий документ.</param>
    /// <param name="version">Исходная версия для преобразования.</param>
    public Sungero.Content.IElectronicDocumentVersions CreatePdfVersionFromVersion(Sungero.Content.IElectronicDocument document, Sungero.Content.IElectronicDocumentVersions version)
    {
      var versionExtension = version.BodyAssociatedApplication.Extension.ToLower();
      if (!Sungero.Docflow.IsolatedFunctions.PdfConverter.CheckIfExtensionIsSupported(versionExtension))
        throw new Exception(Resources.InvalidExtention);
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.GeneratePdf(inputStream, versionExtension);
        }
        catch (AppliedCodeException e)
        {
          Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.InnerException);
          throw new Exception(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.InnerException);
        }
      }
      var existsVersion = document.Versions.Where(x => x.Note == Resources.PrintableVersionNote && x.AssociatedApplication.Extension.ToLower() == "pdf").OrderByDescending(x => x.Id).FirstOrDefault();
      if (existsVersion != null)
        existsVersion.Body.Write(pdfDocumentStream);
      else
      {
        document.CreateVersionFrom(pdfDocumentStream, "pdf");
        existsVersion = document.LastVersion;
        existsVersion.Note = Resources.PrintableVersionNote;
      }
      return existsVersion;
    }
  }
}