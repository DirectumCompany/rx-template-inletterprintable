using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.PrintableTemplate.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Создать версию в формате pdf с штампами.
    /// </summary>
    /// <param name="letter">Входящий документ.</param>
    /// <param name="regStampCoordinates">Координаты штампа рег. данных.</param>
    /// <param name="regStampInResponseToCoordinates">Координаты штампа рег. данных исходного письма.</param>
    /// <param name="signStampCoordinates">Координаты штампа ЭП.</param>
    [Remote]
    public void GenerateNewVersionWithStamp(Sungero.RecordManagement.IIncomingLetter letter, Structures.Module.StampCoordinates regStampCoordinates, Structures.Module.StampCoordinates regStampInResponseToCoordinates, Structures.Module.StampCoordinates signStampCoordinates)
    {
      var version = letter.Versions.Where(x => Signatures.Get(x).Where(s => s.SignatureType == SignatureType.Approval).Any()).OrderByDescending(x => x.Id).FirstOrDefault();
      var newVersion = this.CreatePdfVersionFromVersion(letter, version);
      var pdfDocumentStream = new System.IO.MemoryStream();
      newVersion.Body.Read().CopyTo(pdfDocumentStream);
      var regStamp = this.CreateIncomingRegStamp(letter, regStampCoordinates, Resources.RegDataTemplate);
      pdfDocumentStream = regStamp.AddImageToPDF(pdfDocumentStream);
      if (letter.InResponseTo != null)
      {
        var regStampInResponseTo = this.CreateRegStamp(letter.InResponseTo, regStampInResponseToCoordinates, Resources.RegDataInResponseTo);
        pdfDocumentStream = regStampInResponseTo.AddImageToPDF(pdfDocumentStream);
      }
      
      var signature = Signatures.Get(version)
        .Where(s => s.SignatureType == SignatureType.Approval && s.SignCertificate != null).FirstOrDefault();
      if (signature != null)
      {
        var sigStamp = this.CreateSigStamp(signStampCoordinates, signature);
        if (sigStamp != null)
          pdfDocumentStream = sigStamp.AddImageToPDF(pdfDocumentStream);
      }
      newVersion.Body.Write(pdfDocumentStream);
      pdfDocumentStream.Close();
      letter.Save();
    }
    
    
    /// <summary>
    /// Создание pdf версии документа.
    /// </summary>
    /// <param name="document">Входящий документ.</param>
    /// <param name="version">Исходная версия для преобразования.</param>
    public Sungero.Content.IElectronicDocumentVersions CreatePdfVersionFromVersion(Sungero.Content.IElectronicDocument document, Sungero.Content.IElectronicDocumentVersions version)
    {
      var versionExtension = version.BodyAssociatedApplication.Extension.ToLower();
      if (!Sungero.AsposeExtensions.Converter.CheckIfExtensionIsSupported(versionExtension))
        throw new Exception(Resources.InvalidExtention);
      var pdfConverter = new Sungero.AsposeExtensions.Converter();
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, versionExtension);
        }
        catch (Sungero.AsposeExtensions.PdfConvertException e)
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
    
    /// <summary>
    /// Генерация штампа с рег. данными.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="regStampCoordinates">Координаты штампов.</param>
    /// <param name="templateRegDataDoc">Шаблон штампа.</param>
    /// <returns>Штамп.</returns>
    public virtual MEDOSerializingXML.MEDOStamp CreateRegStamp(Sungero.Docflow.IOfficialDocument document, Structures.Module.StampCoordinates regStampCoordinates, string templateRegDataDoc)
    {
      var regStampInfo = this.GetRegStamp(document, templateRegDataDoc);
      var stampPageType = MEDOSerializingXML.Stamps.StampedPages.FirstPage;
      var stampedPages = new List<int>();
      if (regStampCoordinates.PageNumber > 1)
      {
        stampPageType = MEDOSerializingXML.Stamps.StampedPages.PageRange;
        stampedPages.Add(regStampCoordinates.PageNumber);
      }
      
      return new MEDOSerializingXML.MEDOStamp(regStampInfo,
                                              regStampCoordinates.Horizontally,
                                              regStampCoordinates.Vertically,
                                              regStampCoordinates.Width,
                                              regStampCoordinates.Height,
                                              MEDO.PublicConstants.Module.Border, stampPageType, stampedPages);
    }
    
    /// <summary>
    /// Генерация штампа для входящего.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="regStampCoordinates">Координаты штампов.</param>
    /// <param name="templateRegDataDoc">Шаблон штампа.</param>
    /// <returns>Штамп.</returns>
    public virtual MEDOSerializingXML.MEDOStamp CreateIncomingRegStamp(Sungero.Docflow.IIncomingDocumentBase document, Structures.Module.StampCoordinates regStampCoordinates, string templateRegDataDoc)
    {
      var regStampInfo = this.GetIncomingRegStamp(document, templateRegDataDoc);
      var stampPageType = MEDOSerializingXML.Stamps.StampedPages.FirstPage;
      var stampedPages = new List<int>();
      if (regStampCoordinates.PageNumber > 1)
      {
        stampPageType = MEDOSerializingXML.Stamps.StampedPages.PageRange;
        stampedPages.Add(regStampCoordinates.PageNumber);
      }
      
      return new MEDOSerializingXML.MEDOStamp(regStampInfo,
                                              regStampCoordinates.Horizontally,
                                              regStampCoordinates.Vertically,
                                              regStampCoordinates.Width,
                                              regStampCoordinates.Height,
                                              MEDO.PublicConstants.Module.Border, stampPageType, stampedPages);
    }
    
    /// <summary>
    /// Сформировать текст для штампа рег данных.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="templateRegDataDoc">Шаблон штампа.</param>
    /// <returns>Штамп рег. данных в формате строки.</returns>
    public virtual string GetRegStamp(Sungero.Docflow.IOfficialDocument document, string templateRegDataDoc)
    {
      var regDate = document.RegistrationDate != null ? document.RegistrationDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      var regNum = document.RegistrationNumber;
      var regInfoStamp = templateRegDataDoc.Replace("{RegNum}", regNum).Replace("{RegDate}", regDate);
      return regInfoStamp;
    }
    
    /// <summary>
    /// Сформировать текст для штампа входящего письма.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="templateRegDataDoc">Шаблон штампа.</param>
    /// <returns>Штамп "В ответ на" в формате строки.</returns>
    public virtual string GetIncomingRegStamp(Sungero.Docflow.IIncomingDocumentBase document, string templateRegDataDoc)
    {
      var regDate = document.Dated != null ? document.Dated.Value.ToString("dd.MM.yyyy") : string.Empty;
      var regNum = document.InNumber;
      var regInfoStamp = templateRegDataDoc.Replace("{RegNum}", regNum).Replace("{RegDate}", regDate);
      return regInfoStamp;
    }
    
    /// <summary>
    /// Сформировать штамп подписи.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="sigStampCoordinates">Координаты.</param>
    /// <returns>Штамп.</returns>
    public virtual MEDOSerializingXML.MEDOStamp CreateSigStamp(Structures.Module.StampCoordinates signStampCoordinates, Sungero.Domain.Shared.ISignature signature)
    {
      var base64PngEmblem = string.Empty;
      var commonSetting = MEDO.CommonMedoSettingses.GetAll().FirstOrDefault();
      base64PngEmblem = commonSetting.SignatureStampLogo;
      if (string.IsNullOrEmpty(base64PngEmblem))
        base64PngEmblem = MEDO.PublicConstants.Module.StampEmblem;
      var stampPageType = MEDOSerializingXML.Stamps.StampedPages.FirstPage;
      var stampedPages = new List<int>();
      if (signStampCoordinates.PageNumber > 1)
      {
        stampPageType = MEDOSerializingXML.Stamps.StampedPages.PageRange;
        stampedPages.Add(signStampCoordinates.PageNumber);
      }
      var signatureSerialNumber = signature.SignCertificate.Thumbprint.ToLower();
      var signatureUserName = signature.SignatoryFullName;
      var signatureDates = string.Format("с {0} по {1}", signature.SignCertificate.NotBefore.Value.ToString("dd.MM.yyyy"), signature.SignCertificate.NotAfter.Value.ToString("dd.MM.yyyy"));
      return new MEDOSerializingXML.MEDOStamp(base64PngEmblem, signatureSerialNumber, signatureUserName, signatureDates,
                                              signStampCoordinates.Horizontally,
                                              signStampCoordinates.Vertically,
                                              signStampCoordinates.Width,
                                              signStampCoordinates.Height, stampPageType, stampedPages);
    }

  }
}