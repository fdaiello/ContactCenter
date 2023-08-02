using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    // Estages of Board
    public class StageDto
    {
        public StageDto(Stage stage)
        {
            // Copy all writable properties that exists in StageDto from Stage - Except Cards
            foreach (PropertyInfo property in typeof(StageDto).GetProperties().Where(p => p.CanWrite))
            {
                var x = stage.GetType().GetProperty(property.Name).GetValue(stage, null);
                property.SetValue(this, x, null);
            }

            // Initialize CardDto collection of this Stage
            this.Cards = new Collection<CardDto>();

            // Copies all Cards from stage received as parameter, to this new StageDto
            if (stage.Cards != null)
            {
                foreach (Card card in stage.Cards)
                {
                    this.AddCard(card);
                }
            }
        }
        public int Id { get; set; }                                         // Primary Key
        public int BoardId { get; set; }                                    // Foreing Key to Board
        public string Name { get; set; }                                   // Name
        public string Label { get; set; }                                  // Label, shorten name
        public int Order { get; set; }
        public int ShowMax { get; set; }                                   // When getting cards from stage, Take Top ( showmax )

        public ICollection<CardDto> Cards { get; }                        // Collection of cards
        public void AddCard( Card card )
        {
            this.Cards.Add(new CardDto(card) );
        }
    }
}
