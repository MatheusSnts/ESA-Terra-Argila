// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function updatePreviewImages(event) {
    let imageSlots = document.querySelectorAll(".image-preview")
    for (let i = 0; i < event.target.files.length; i++) {
        let slot = imageSlots[i];
        if (!slot) {
            slot = document.createElement("img");
            slot.classList.add("col-md-5", "image-preview");
            document.querySelector("#previews-container").appendChild(slot);
            slot.src = URL.createObjectURL(event.target.files[i]);
        }
        else
            imageSlots[i].src = URL.createObjectURL(event.target.files[i]);
    }
}

function switchToImage(event) {
    let mainImageElement = document.getElementById('main-image');
    let currentMainImageSrc = mainImageElement.src;
    mainImageElement.src = event.target.src;
    event.target.src = currentMainImageSrc;
}

function submitFormOnChange(event)
{
    $(event.target).closest("form").submit();
}

function setFavorite(event)
{
    let id = $(event.target).data("id");
    let isFavorite = $(event.target).data("value");
    console.log(id, isFavorite)
    $.ajax({
        url: "/Materials/SetFavorite",
        type: "POST",
        data: JSON.stringify({ id, isFavorite }),
        contentType: "application/json",
        success: function (response) {
            if (!response.success) return;
            if (isFavorite) {
                event.target.src = event.target.src.replace("off", "on")
                $(event.target).data("value", false)
            }
            else {
                event.target.src = event.target.src.replace("on", "off")
                $(event.target).data("value", true)

            }
        },
        error: function (xhr) {
            console.error("Erro ao atualizar favorito:", xhr.responseText);
        }
    });
    
}
function addCartItemQuantity(event)
{
    ajaxAddCartItemQuantity($(event.target).closest(".cart-item").data("id"), 1, event);
}

function removeCartItemQuantity(event)
{
    ajaxAddCartItemQuantity($(event.target).closest(".cart-item").data("id"), -1, event);
}


function ajaxAddCartItemQuantity(id, value, event)
{
    $.ajax({
        url: "/Orders/AddQuantity",
        type: "POST",
        data: JSON.stringify({ id, value }),
        contentType: "application/json",
        success: function (response) {
            if (!response.success) return;
            let cartItem = $(event.target).closest(".cart-item");
            cartItem.find(".quantity").text(response.quantity);
            cartItem.find(".partial").text(response.partial);
            $(".total").text(response.total);
        },
        error: function (xhr) {
            console.error("Erro ao atualizar quantidade:", xhr.responseText);
        }
    });
}

function updateCartItemCount()
{
    console.log("Atualizando quantidade!")
    $.ajax({
        url: "/Orders/GetCartItemCount",
        type: "GET",
        success: function (response) {
            console.log(response)
            if (!response.success || !response.count) return;
            let cartCount = $(".cart-item-count");
            cartCount.text(response.count);
            cartCount.removeClass('hidden');
        },
        error: function (xhr) {
            console.error("Erro ao atualizar quantidade:", xhr.responseText);
        }
    });

}
