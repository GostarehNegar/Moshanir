using Mapna.Transmittals.Exchange.Internals;
using System.IO;

namespace Mapna.Transmittals.Exchange.Services.Queues
{
    public class IncommingTransmitalContext : TransmittalProcessingContext<IncommingTransmitalContext>
    {
        public IncommingTransmitalContext()
        {
            MaxTrials = 3;
        }
        public override string Title => $"Receiving {base.Title}";
        public string GetDestinationFileName(TransmittalFileSubmitModel file)
        {
            return Path.Combine(Path.GetTempPath(), Transmittal.TR_NO, file.FileName);
        }
        public string GetSharePointDestinationPath(TransmittalFileSubmitModel file)
        {
            if (string.IsNullOrWhiteSpace(this.TransmittalItem?.TransmittalNo))
            {
                throw new ValidationException(
                    $"Invalid Transmittal Number. Nonempty transmittal numbers are required to come up with a valid path in DocLib.");
            }
            return $"/{this.TransmittalItem?.TransmittalNo}/{file.FileName}";
        }


    }
}
