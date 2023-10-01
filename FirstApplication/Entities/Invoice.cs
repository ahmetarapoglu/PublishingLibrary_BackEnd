﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.ComponentModel;

namespace BookShop.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
        public bool IsInvoiced { get; set; }

        public class InvoiceConfig : IEntityTypeConfiguration<Invoice>
        {
            public void Configure(EntityTypeBuilder<Invoice> builder)
            {

                //------------------------//
                //One To One RelationShip :
                //------------------------//

                //Order - Invoice.
                builder.HasOne<Order>(i => i.Order)
                        .WithOne(o => o.Invoice)
                        .HasForeignKey<Invoice>(i => i.OrderId);

                builder.Property(o => o.IsInvoiced)
                       .HasDefaultValue(true);
            }
        }
    }
}