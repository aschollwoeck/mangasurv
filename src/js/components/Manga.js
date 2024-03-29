import React from "react";
import { IndexLink, Link } from "react-router";

import * as MangaActions from "../actions/MangaActions";
var moment = require("moment");

export default class Manga extends React.Component {
  constructor(props, context) {
      super(props, context);
      this.state = {
        auth: props.auth
      };
  }

  followManga() {
    MangaActions.followManga(this.props);
  }

  unfollowManga() {
    MangaActions.unfollowManga(this.props);
  }

  markAsRead() {
    MangaActions.markAsRead(this.props);
  }

  render() {

    // const { id, name, chapters, followed, lastupdate, image, chapterUpdates } = this.props;
    // const imagePath = "images/" + this.props.image;
    const imagePath = "images/" + this.props.fileSystemName.replace(/[ :]/g, "_") + ".jpg";

    const linkDetails = "mangas/" + this.props.id;
    console.log(linkDetails);

    return (
      <div class={ this.props.chapterUpdates ? "overview overviewWide" : "overview " }>
        <div class={ this.props.chapterUpdates ? "divHalfHorizontal overviewThumb" : "overviewThumb" }>
          {
            this.props.followed ? 
            <button class="btn-danger hoverdeleteoverview" onClick={this.unfollowManga.bind(this)}>- Unfollow</button>
            :
            <button class="btn-success hoveraddoverview" onClick={this.followManga.bind(this)}>+ Follow</button> 
          }
          <Link to={linkDetails}>
            <img src={imagePath}/>
          </Link>
          <div>
            <p>{ this.props.name }</p>
            {/*<table>
              <thead>
                <tr>
                  <th colSpan="3">{ this.props.name }</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Chapters:</td>
                  <td>{ this.props.chapters.length }</td>
                </tr>
                <tr>
                  <td>Last Update:<br/></td>
                  { 
                    moment(this.props.lastupdate).format("YYYY") > "0001" ? 
                      <td>{ moment(this.props.lastupdate).format("ll") }({moment(this.props.lastupdate).startOf("hour").fromNow()})</td>
                    :
                       <td></td>
                  }
                </tr>
              </tbody>
            </table>*/}
          </div>
        </div>
        {
          this.props.chapterUpdates ?
          <div class="divHalfHorizontal divOverviewUpdates">
            <div>
              <table>
                <tbody>
                  {this.props.chapterUpdates.map((chapter) => {
                    return <tr><td><a href={chapter.address} target="blank">{chapter.chapterNo} {moment(chapter.enterDate).format("ll")}</a></td></tr>;
                  })}
                </tbody>
              </table>
            </div>
            <button class="btn btn-success" onClick={this.markAsRead.bind(this)}>Mark as read</button>
          </div>
          :
          ""
        }
      </div>
    );
  }
}