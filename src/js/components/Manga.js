import React from "react";

export default class Manga extends React.Component {
  constructor(props) {
    super();
  }

  render() {
    
    const { id, name, chapters, followed, lastupdate, image } = this.props;
    const imagePath = "images/" + image + ".jpg";

    const imageStyle = {
      height: "170px",
      width: "100%",
    };

    const mangaStyle = {
      width: "10em",
      height: "16em",
      border: 1,
    }

    const nameStyle = {
      width: "10em",
      height: "3em",
      display: "table-cell",
      verticalAlign: "middle",
      textAlign: "center",
    }

    return (
      <div style={mangaStyle}>
        <img src={imagePath} style={imageStyle}/>
        <div style={nameStyle}>
          <span>{ name }</span>
          <p>Chapters: { chapters }</p>
          <p>Last Update: { lastupdate }</p>
        </div>
      </div>
    );
  }
}