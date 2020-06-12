using System;
namespace AWS.MessageService.KINESIS.Dominio
{
    public class Peca
    {
        public Peca()
        {
        }


        public int id_t_gaiola { get; set; }
        public string nome { get; set; }
        public double diametro { get; set; }
        public DateTime data { get; set; }
    }
}
