public interface IContainSender
{
    ISender Sender { get; set; }
    void SetSender(ISender sender);
}


