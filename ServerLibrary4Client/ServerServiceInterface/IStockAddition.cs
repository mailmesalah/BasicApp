using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface IStockAddition
    {
        [OperationContract]
        bool CreateBill(CStockAddition oStockAddition);
        [OperationContract]
        CStockAddition ReadBill(string billNo,string financialCode);
        [OperationContract]
        bool UpdateBill(CStockAddition oStockAddition);
        [OperationContract]
        bool DeleteBill(string billNo,string financialCode);        
        [OperationContract]
        int ReadNextBillNo(string financialCode);
     
    }


    
    [DataContract]
    public class CStockAddition
    {
        int id;
        string billNo;
        DateTime billDateTime = new DateTime();
        string narration;
        string financialCode;
        List<CStockAdditionDetails> details= new List<CStockAdditionDetails>();
 
        [DataMember]
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public string BillNo
        {
            get { return billNo; }
            set { billNo = value; }
        }
        
        [DataMember]
        public DateTime BillDateTime
        {
            get { return billDateTime; }
            set { billDateTime = value; }
        }
                
        [DataMember]
        public string Narration
        {
            get { return narration; }
            set { narration = value; }
        }

        [DataMember]
        public string FinancialCode
        {
            get { return financialCode; }
            set { financialCode = value; }
        }

        [DataMember]
        public List<CStockAdditionDetails> Details
        {
            get { return details; }
            set { details = value; }
        }
    }


    [DataContract]
    public class CStockAdditionDetails
    {
        int serialNo;
        string productCode;
        string product;
        string stockAdditionUnit;
        string stockAdditionUnitCode;
        decimal stockAdditionUnitValue;
        decimal quantity;
        decimal stockAdditionRate;
        decimal mrp;
        string barcode;
        decimal total;

        [DataMember]
        public int SerialNo
        {
            get { return serialNo; }
            set { serialNo = value; }
        }

        [DataMember]
        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        [DataMember]
        public string Product
        {
            get { return product; }
            set { product = value; }
        }

        [DataMember]
        public string StockAdditionUnit
        {
            get { return stockAdditionUnit; }
            set { stockAdditionUnit = value; }
        }

        [DataMember]
        public string StockAdditionUnitCode
        {
            get { return stockAdditionUnitCode; }
            set { stockAdditionUnitCode = value; }
        }

        [DataMember]
        public decimal StockAdditionUnitValue
        {
            get { return stockAdditionUnitValue; }
            set { stockAdditionUnitValue = value; }
        }

        [DataMember]
        public decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        [DataMember]
        public decimal StockAdditionRate
        {
            get { return stockAdditionRate; }
            set { stockAdditionRate = value; }
        }

        [DataMember]
        public decimal MRP
        {
            get { return mrp; }
            set { mrp = value; }
        }

        [DataMember]
        public string Barcode
        {
            get { return barcode; }
            set { barcode = value; }
        }

        [DataMember]
        public decimal Total
        {
            get { return total; }
            set { total = value; }
        }
    }
}
