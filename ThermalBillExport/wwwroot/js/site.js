function generateBill() {
    let products = [];
    let totalAmount = 0;

    // Duyệt qua các sản phẩm
    document.querySelectorAll('.product').forEach(productElement => {
        const name = productElement.dataset.name;
        const price = parseInt(productElement.dataset.price);
        const quantity = parseInt(productElement.querySelector('.quantity').value);

        if (quantity > 0) {
            const total = price * quantity;

            products.push({
                name: name,
                quantity: quantity,
                price: price,
                total: total
            });

            totalAmount += total;
        }
    });

    $.ajax({
        url: '/Bill/Generate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            products: products,
            totalAmount: totalAmount
        }),
        success: function (response) {
            // Nhận lại HTML từ controller và render ở bên phải
            document.querySelector('.bill-container').innerHTML = response;
        },
        error: function (xhr, status, error) {
            console.error('Lỗi khi gửi dữ liệu:', error);

            alert('Có lỗi xảy ra trong quá trình gửi dữ liệu. Vui lòng thử lại sau.');

            console.log('Chi tiết lỗi:', {
                xhr: xhr,
                status: status,
                error: error
            });
        }
    });

}
function downloadBill() {
    var billNumber = $('#printBillBtn').data('bill-number');
    var convenienceFee = parseFloat($('#printBillBtn').data('convenience-fee')) || 0;
    var amountDue = parseFloat($('#printBillBtn').data('amount-due')) || 0;
    var productsData = $('#printBillBtn').data('products');

    if (!productsData || productsData.length === 0) {
        console.error("Không có sản phẩm nào để in hóa đơn.");
        alert("Không có sản phẩm nào để in hóa đơn. Vui lòng kiểm tra lại.");
        return;
    }

    // Tạo form tạm thời để gửi yêu cầu tải file
    var form = $('<form></form>').attr({
        method: 'POST',
        action: '/Bill/PrintBill'
    });

    // Thêm dữ liệu vào form
    form.append($('<input>').attr({
        type: 'hidden',
        name: 'BillNumber',
        value: billNumber
    }));
    form.append($('<input>').attr({
        type: 'hidden',
        name: 'ConvenienceFee',
        value: convenienceFee
    }));
    form.append($('<input>').attr({
        type: 'hidden',
        name: 'AmountDue',
        value: amountDue
    }));
    form.append($('<input>').attr({
        type: 'hidden',
        name: 'Products',
        value: JSON.stringify(productsData)
    }));

    // Gửi form và tải về file
    form.appendTo('body').submit();
}
