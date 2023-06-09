using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// NOTA: O Controlador dos Animais também vai funcionar para ser o controlador das Fotografias

namespace CriadorDeCaes.Models {
   /// <summary>
   /// Dados dos animais
   /// </summary>
   public class Animais {

      public Animais() {
         // inicializar a lista de fotografias
         // associada ao cão/cadela
         ListaFotografias=new HashSet<Fotografias>();
      }

      /// <summary>
      /// PK
      /// </summary>
      public int Id { get; set; }

      /// <summary>
      /// Nome do animal
      /// </summary>
      [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
      public string Nome { get; set; }

      /// <summary>
      /// Sexo do animal
      /// M - macho
      /// F - Fêmea
      /// </summary>
      public string Sexo { get; set; }

        /// <summary>
        /// Data de nascimento
        /// </summary>
        [Display(Name ="Data de Nascimento")]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório")]
      public DateTime DataNasc { get; set; }

        /// <summary>
        /// data de compra do animal
        /// Se nulo, o animal nasceu nas instalações do criador
        /// A adição do ? torna o atributo facultativo mas implica uma migração visto ser uma alteração À BD
        /// </summary>
        [Display(Name = "Data de Compra")]
      public DateTime? DataCompra { get; set; }

        /// <summary>
        /// Preço de Compra do Animal
        /// </summary>
        public decimal PrecoCompra { get; set;}

        /// <summary>
        /// cmapo auxiliar de introdução do Preço de Compra do Animal
        /// ? - zero ou uma vez
        /// NotMapped - este anotador impede este atributo de ser exportado para a base de dados
        /// </summary>
        [Display(Name = "Preço de Compra")]
        [RegularExpression("[1-9]1[0-9][.,]?[0-9]{0-2}", ErrorMessage =" No {0} só pode utilizar algarismos, e se desejar, duas casas decimais no final")]
        [NotMapped]
        public string PrecoCompraAux { get; set; }

        /// <summary>
        /// número de registo no LOP
        /// </summary>
        [Display(Name ="Registo LOP")]
      public string RegistoLOP { get; set; }

      // ****************************************
      // Criação das chaves forasteiras
      // ****************************************

      /// <summary>
      /// Lista das fotografias associadas a um animal
      /// </summary>
      public ICollection<Fotografias> ListaFotografias { get; set; }

      /// <summary>
      /// FK para a identificação da Raça do animal
      /// </summary>
      [ForeignKey(nameof(Raca))]
        [Display(Name = "Raça")]
      public int RacaFK { get; set; }
        [Display(Name ="Raça")]
      public Racas Raca { get; set; }

      /// <summary>
      /// FK para o Criador dono do animal
      /// </summary>
      [ForeignKey(nameof(Criador))]
        [Display(Name ="Criador")]
      public int CriadorFK { get; set; }
        [Display(Name ="Criador")]
      public Criadores Criador { get; set; }

   }
}
