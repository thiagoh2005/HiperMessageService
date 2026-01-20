namespace HiperMessageService
{
    public class PedidoEstoqueMessageCommand
    {
        public Guid Id { get; set; }
        public string Responsavel { get; set; }
        public string Descricao { get; set; }
        public Guid ItemId { get; set; }
        public string Item { get; set; }
        public DateTime DataEntregue { get; set; }
        public int Unidades { get; set; }
    }
}
