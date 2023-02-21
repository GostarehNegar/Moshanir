using System;

namespace Mapna.Transmittals.Exchange
{
    
    public class TransmittalFileSubmitModel
    {
        public string DocNumber { get; set; }
        public string Url { get; set; }
        public string Ext_Rev { get; set; }
        public string Source_Id => Url;
        public string Int_Rev { get; set; }
        public string Status { get; set; }
        public string Purpose { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return $"{DocNumber}";
        }
        public TransmittalFileSubmitModel Validate()
        {
            void assert(bool predicate, Func<string> message)
            {
                if (!predicate)
                    throw new ValidationException($"Invalid Transmittal File: {this}. {message()}");
            }
            assert(!string.IsNullOrWhiteSpace(FileName), () => $"'{FileName}' is not a valid '{nameof(TransmittalFileSubmitModel.FileName)}' or is null.");
            assert(Uri.IsWellFormedUriString(Url, UriKind.Absolute), () => $"'{Url}' is not a valid Url");
            assert(!string.IsNullOrWhiteSpace(Status), () => $"'{Status}' is not a valid {nameof(TransmittalFileSubmitModel.Status)} or is null");
            assert(!string.IsNullOrWhiteSpace(Purpose), () => $"'{Purpose}' is not a valid {nameof(TransmittalFileSubmitModel.Purpose)} or is null");
            assert(!string.IsNullOrWhiteSpace(Int_Rev), () => $"'{Int_Rev}' is not a valid {nameof(TransmittalFileSubmitModel.Int_Rev)} or is null");
            assert(!string.IsNullOrWhiteSpace(Ext_Rev), () => $"'{Ext_Rev}' is not a valid {nameof(TransmittalFileSubmitModel.Ext_Rev)} or is null");
            return this;
        }
    }

}
