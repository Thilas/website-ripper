using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    public sealed class DocumentTypeReference : Reference
    {
        readonly XmlDocument _document;
        XmlDocumentType _documentType;

        public DocumentTypeReference(Parser parser, XmlDocument document)
            : base(new ReferenceArgs(parser, document.DocumentType != null ? ReferenceKind.ExternalResource : ReferenceKind.Skip))
        {
            _document = document;
            _documentType = document.DocumentType;
        }

        protected override string UriInternal
        {
            get { return _documentType.SystemId; }
            set
            {
                var documentType = _document.CreateDocumentType(_documentType.Name, _documentType.PublicId, value, _documentType.InternalSubset);
                _document.ReplaceChild(documentType, _documentType);
                _documentType = documentType;
            }
        }
    }
}
