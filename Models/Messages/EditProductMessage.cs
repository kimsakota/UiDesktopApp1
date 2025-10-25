﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyKhoHang.Models.Messages
{
    public sealed class EditProductMessage
    {
        public int ProductId { get; }
        public EditProductMessage(int productId) => ProductId = productId;
    }
}
