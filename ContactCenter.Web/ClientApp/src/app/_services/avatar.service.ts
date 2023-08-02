import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AvatarService {

  constructor() { }

  /**
   * Devolve o prefixo para montar icones para quem não tem pictureprofile
   */
  getIconPrefix(name: string): string {

    const blankIcon = " ";
    var icon_name;

    if (name == "" || name == blankIcon || name == null) {
      icon_name = blankIcon;
    }
    else {
      if (name == null)
        name = "";
      else
        name = name.trim();

      var arr = name.split(" ");
      var first_name = arr[0];
      var firstchars = [...first_name];
      if (firstchars.length > 0) {
        var first_letter = firstchars[0];
        icon_name = first_letter; 
        if (!icon_name.match(/[a-z]/i)) {
          icon_name = blankIcon;

        } else {
          if (arr.length > 1) {
            var second_name = arr[1];
            var nexchars = [...second_name];
            icon_name += nexchars[0];
          }
          icon_name = icon_name.toUpperCase();
        }
      }
      else {
        icon_name = blankIcon;
      }
    }

    return icon_name;
  }

  /**
   * Devolve o sufixo da classe para dar cor ao icone gerado
   */
  getIconClass(name: string): string {

    var icon_color;

    if (name == "" || name == "?" || name == " " || name == null) {
      icon_color = "DARK";
    }
    else {
      if (name == null)
        name = "";
      else
        name = name.trim();

      var arr = name.split(" ");
      var color_arr = (new String("PRIMARY SECONDARY SUCCESS INFO WARNING DANGER FOCUS DARK")).toLowerCase().split(" ");
      var first_name = arr[0];
      var firstchars = [...first_name];
      if (firstchars.length > 0) {
        var first_letter = firstchars[0];
        if (arr.length > 1) {
          var second_name = arr[1];
        }
        icon_color = color_arr[(first_letter.charCodeAt(0) - 65) % color_arr.length];
      }
      else {
        icon_color = "DARK";
      }
    }

    return icon_color;
  }

}
