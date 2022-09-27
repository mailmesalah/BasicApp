using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using WpfAccountServerApp.General;

namespace WpfAccountServerApp.Services
{   
    public class PurchaseReturnService : IPurchaseReturn
    {
        private string mBillType = "PR";

        public bool CreateBill(CPurchaseReturn oPurchaseReturn)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        ProductService ls = new ProductService();
                        BillNoService bs = new BillNoService();


                        int cbillNo = bs.ReadNextPurchaseReturnBillNo(oPurchaseReturn.FinancialCode);
                        bs.UpdatePurchaseReturnBillNo(oPurchaseReturn.FinancialCode,cbillNo+1);

                        List<string> barcodes = new BarcodeService().ReadBarcodes(oPurchaseReturn.Details.Count);
                        
                        for (int i = 0; i < oPurchaseReturn.Details.Count; i++)
                        {
                            product_transactions pt = new product_transactions();

                            pt.bill_no= cbillNo.ToString();
                            pt.bill_type = mBillType;                            
                            pt.bill_date_time = oPurchaseReturn.BillDateTime;
                            pt.ref_bill_no = oPurchaseReturn.RefBillNo;
                            pt.ref_bill_date_time = oPurchaseReturn.RefBillDateTime;
                            pt.supplier_code = oPurchaseReturn.SupplierCode;
                            pt.supplier = oPurchaseReturn.Supplier;
                            pt.supplier_address = oPurchaseReturn.SupplierAddress;
                            pt.narration = oPurchaseReturn.Narration;
                            pt.advance = oPurchaseReturn.Advance;
                            pt.extra_charges = oPurchaseReturn.Expense;
                            pt.discounts = oPurchaseReturn.Discount;
                            pt.financial_code = oPurchaseReturn.FinancialCode;

                            pt.serial_no = oPurchaseReturn.Details.ElementAt(i).SerialNo;
                            pt.product_code = oPurchaseReturn.Details.ElementAt(i).ProductCode;
                            pt.product = oPurchaseReturn.Details.ElementAt(i).Product;
                            pt.purchase_unit = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnit;
                            pt.purchase_unit_code = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitCode;
                            pt.purchase_unit_value = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitValue;
                            pt.quantity = oPurchaseReturn.Details.ElementAt(i).Quantity*-1;
                            pt.purchase_rate = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnRate;
                            pt.mrp = oPurchaseReturn.Details.ElementAt(i).MRP;
                            //get a barcode here
                            pt.barcode = barcodes.ElementAt(i);
                            pt.unit_code = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitCode;
                            pt.unit_value = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitValue;

                            dataB.product_transactions.Add(pt);                            
                        }

                        dataB.SaveChanges();
                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }                
            }

            return returnValue;
        }

        public bool DeleteBill(string billNo,string financialCode)
        {
            bool returnValue = true;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        //Delete the transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType);
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        dataB.SaveChanges();                        
                        dataBTransaction.Commit();
                    }
                    catch
                    {
                        returnValue = false;
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }
            }
            return returnValue;
        }

        public CPurchaseReturn ReadBill(string billNo,string financialCode)
        {
            CPurchaseReturn ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode&&x.bill_type==mBillType).OrderBy(y=>y.serial_no);
                
                if (cps.Count() > 0)
                {
                    ccp = new CPurchaseReturn();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.RefBillNo = cp.ref_bill_no;
                    ccp.RefBillDateTime = (DateTime)cp.ref_bill_date_time;
                    ccp.SupplierCode = cp.supplier_code;
                    ccp.Supplier = cp.supplier;
                    ccp.SupplierAddress = cp.supplier_address;
                    ccp.Narration = cp.narration;
                    ccp.Advance = (decimal)cp.advance;
                    ccp.Expense = (decimal)cp.extra_charges;
                    ccp.Discount = (decimal)cp.discounts;
                    ccp.FinancialCode = cp.financial_code;
                    
                    foreach (var item in cps)
                    {
                        decimal quantity = ReadAvailableQuantityOfBarcode(item.barcode, item.purchase_unit_code, (decimal)item.purchase_unit_value,billNo,financialCode);
                        ccp.Details.Add(new CPurchaseReturnDetails() { SerialNo = (int)item.serial_no, ProductCode = item.product_code, Product = item.product, PurchaseReturnUnit = item.purchase_unit, PurchaseReturnUnitCode = item.purchase_unit_code, PurchaseReturnUnitValue = (decimal)item.purchase_unit_value, Quantity = (decimal)item.quantity*-1, PurchaseReturnRate = (decimal)item.purchase_rate, MRP = (decimal)item.mrp, Total = (decimal)(item.quantity * item.purchase_rate*-1), Barcode = item.barcode, OldQuantity= quantity });                        
                    }
                }
                
            }

            return ccp;
        }

        public CPurchaseReturn ReadPurchaseBill(string billNo, string financialCode)
        {
            CPurchaseReturn ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == "P").OrderBy(y => y.serial_no);

                if (cps.Count() > 0)
                {
                    ccp = new CPurchaseReturn();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.SupplierCode = cp.supplier_code;
                    ccp.Supplier = cp.supplier;
                    ccp.SupplierAddress = cp.supplier_address;
                    ccp.Narration = cp.narration;
                    ccp.Advance = (decimal)cp.advance;
                    ccp.Expense = (decimal)cp.extra_charges;
                    ccp.Discount = (decimal)cp.discounts;
                    ccp.FinancialCode = cp.financial_code;

                    int serialNo = 1;
                    foreach (var item in cps)
                    {
                        decimal quantity = ReadAvailableQuantityOfBarcode(item.barcode, item.purchase_unit_code, (decimal)item.purchase_unit_value,billNo,financialCode);
                        if (quantity > 0)
                        {
                            ccp.Details.Add(new CPurchaseReturnDetails() { SerialNo = serialNo++, ProductCode = item.product_code, Product = item.product, PurchaseReturnUnit = item.purchase_unit, PurchaseReturnUnitCode = item.purchase_unit_code, PurchaseReturnUnitValue = (decimal)item.purchase_unit_value, Quantity = quantity, PurchaseReturnRate = (decimal)item.purchase_rate, MRP = (decimal)item.mrp, Total = (decimal)(quantity * item.purchase_rate), Barcode = item.barcode, OldQuantity = quantity });
                        }
                    }
                }

            }

            return ccp;
        }

        public int ReadNextBillNo(string financialCode)
        {
            
            BillNoService bns = new BillNoService();
            return bns.ReadNextPurchaseReturnBillNo(financialCode);
            
        }

        public bool UpdateBill(CPurchaseReturn oPurchaseReturn)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oPurchaseReturn.BillNo&& x.financial_code==oPurchaseReturn.FinancialCode&&x.bill_type==mBillType);
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        int serialNo = 1;
                        for (int i = 0; i < oPurchaseReturn.Details.Count; i++)
                        {

                            product_transactions pt = new product_transactions();

                            pt.bill_no = oPurchaseReturn.BillNo;
                            pt.bill_type = mBillType;
                            pt.bill_date_time = oPurchaseReturn.BillDateTime;
                            pt.ref_bill_no = oPurchaseReturn.RefBillNo;
                            pt.ref_bill_date_time = oPurchaseReturn.RefBillDateTime;
                            pt.supplier_code = oPurchaseReturn.SupplierCode;
                            pt.supplier = oPurchaseReturn.Supplier;
                            pt.supplier_address = oPurchaseReturn.SupplierAddress;
                            pt.narration = oPurchaseReturn.Narration;
                            pt.advance = oPurchaseReturn.Advance;
                            pt.extra_charges = oPurchaseReturn.Expense;
                            pt.discounts = oPurchaseReturn.Discount;
                            pt.financial_code = oPurchaseReturn.FinancialCode;

                            pt.serial_no = serialNo++;
                            pt.product_code = oPurchaseReturn.Details.ElementAt(i).ProductCode;
                            pt.product = oPurchaseReturn.Details.ElementAt(i).Product;
                            pt.purchase_unit = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnit;
                            pt.purchase_unit_code = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitCode;
                            pt.purchase_unit_value = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitValue;
                            pt.quantity = oPurchaseReturn.Details.ElementAt(i).Quantity * -1;
                            pt.purchase_rate = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnRate;
                            pt.mrp = oPurchaseReturn.Details.ElementAt(i).MRP;
                            pt.barcode = oPurchaseReturn.Details.ElementAt(i).Barcode;
                            pt.unit_code = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitCode;
                            pt.unit_value = oPurchaseReturn.Details.ElementAt(i).PurchaseReturnUnitValue;

                            dataB.product_transactions.Add(pt);

                        }
                        
                        dataB.SaveChanges();

                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {
                    }
                }
            }
            return returnValue;
        }

        private decimal ReadAvailableQuantityOfBarcode(string barcode,string unitCode,decimal unitValue,string billNo,string fCode)
        {
            decimal quanity = 0;
            try {

                UnitService us = new UnitService();
                decimal lowestUnitVal = us.ReadLowestUnitValue(unitCode);

                using (var dataB = new Database9001Entities())
                {
                    var cps = dataB.product_transactions.Where(c=>c.barcode==barcode&&!(c.bill_no==billNo&&c.bill_type==mBillType&&c.financial_code==fCode)).GroupBy(x=>new {x.unit_code},x=>new { x.unit_code, x.unit_value, x.quantity })
                        .Select(y=>new { UnitCode=y.Key.unit_code,Quantity=y.Sum(x=>x.quantity),UnitValue=y.FirstOrDefault().unit_value});
                    foreach (var item in cps)
                    {                        
                        quanity += (lowestUnitVal / item.UnitValue) * (decimal)item.Quantity;
                    }
                    
                }

                quanity /= (lowestUnitVal/unitValue);

            }
            catch
            {                
            }

            return quanity;
        }

        
    }
}
